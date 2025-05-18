using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Liman.Implementation.ServiceCollections
{
    [LimanService(LimanServiceLifetime.Singleton)]
    internal class LimanServiceCollection : ILimanServiceCollection
    {
        private readonly Dictionary<Type, List<LimanImplementation>> implementationsByService = [];
        private readonly Dictionary<Type, List<LimanImplementation>> genericImplementationsByService = [];
        private readonly Dictionary<Type, LimanImplementation> implementationByType = [];
        private readonly List<LimanImplementation> applicationServices = [];
        private readonly List<Assembly> addedAssemblies = [];

        public LimanServiceCollection()
        {
            Add(Assembly.GetExecutingAssembly());
        }

        public void Add(Type implementationType)
        {
            if (!TryAdd(implementationType))
            {
                throw new LimanException($"Type '{implementationType.GetReadableName()}' is missing {nameof(LimanServiceAttribute)}");
            }
        }

        public void Add(Type implementationType, LimanServiceLifetime lifetime, Delegate? constructor = null)
        {
            Add(implementationType, lifetime, [], constructor);
        }

        public void Add(Type implementationType, LimanServiceLifetime lifetime, IEnumerable<Type> serviceTypes, Delegate? constructor = null)
        {
            if (implementationByType.ContainsKey(implementationType)) return;

            if (!serviceTypes.Any())
            {
                serviceTypes = implementationType.GetImplementedTypes();

                if (implementationType.IsGenericTypeDefinition)
                {
                    serviceTypes = serviceTypes.Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition());
                }

                serviceTypes = serviceTypes.Append(implementationType);
            }

            var implementation = new LimanImplementation(implementationType, lifetime, constructor);

            Add(implementation, serviceTypes);
        }

        public void Add(IServiceCollection serviceCollection)
        {
            foreach (var service in serviceCollection)
            {
                Add(service);
            }
        }

        private void Add(ServiceDescriptor service)
        {
            if (service.IsKeyedService)
            {
                throw new LimanException($"Keyed service '{service.ServiceType.GetReadableName()}' is not supported.");
            }

            var implementationType = service.ImplementationType ?? service.ServiceType;

            if (!implementationByType.TryGetValue(implementationType, out var implementation))
            {
                var factoryMethod = service.ImplementationInstance != null
                    ? () => service.ImplementationInstance
                    : (Delegate?)service.ImplementationFactory;

                implementation = new LimanImplementation(implementationType, ToLifetime(service.Lifetime), factoryMethod);
            }

            if (implementationType.IsGenericTypeDefinition)
            {
                if (!service.ServiceType.IsGenericTypeDefinition) throw new LimanException($"Service implementation '{implementation}' cannot implement service '{service.ServiceType.GetReadableName()}', because the service is not generic");
                genericImplementationsByService.AddItem(service.ServiceType, implementation);
            }
            else
            {
                if (service.ServiceType.IsGenericTypeDefinition) throw new LimanException($"Service implementation '{implementation}' cannot implement service '{service.ServiceType.GetReadableName()}', because the service is generic");
                implementationsByService.AddItem(service.ServiceType, implementation);
            }
        }

        public void Add(Assembly assembly, params Type[] exceptions)
        {
            if (addedAssemblies.Contains(assembly)) return;

            addedAssemblies.Add(assembly);

            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (exceptions.Contains(type))
                {
                    continue;
                }

                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null) continue;

                if (type.IsClass && !type.IsAbstract)
                {
                    TryAdd(type);
                }
            }
        }

        public bool TryGetSingle(Type serviceType, [MaybeNullWhen(false)] out ILimanImplementation serviceImplementation)
        {
            var implementations = GetAll(serviceType).ToArray();

            if (implementations.Length == 1)
            {
                serviceImplementation = implementations[0];
                return true;
            }
            else if (implementations.Length == 0)
            {
                serviceImplementation = null;
                return false;
            }
            else
            {
                var implementationsText = string.Join(", ", implementations.Select(x => x.ToString()));
                throw new LimanException($"Service '{serviceType.GetReadableName()}' could not be injected, because multiple implementations were found: {implementationsText}");
            }
        }

        public IEnumerable<ILimanImplementation> GetAll(Type serviceType)
        {
            if (implementationsByService.TryGetValue(serviceType, out var implementations))
            {
                foreach (var implementation in implementations)
                {
                    yield return implementation;
                }
            }

            if (serviceType.IsGenericType)
            {
                var genericServiceType = serviceType.GetGenericTypeDefinition();

                if (genericImplementationsByService.TryGetValue(genericServiceType, out var genericImplementations))
                {
                    foreach (var genericImplementation in genericImplementations)
                    {
                        var implementationType = genericImplementation.Type.MakeGenericType(serviceType.GetGenericArguments());

                        if (!implementationByType.TryGetValue(implementationType, out var implementation))
                        {
                            implementation = new LimanImplementation(implementationType, genericImplementation.Lifetime, null);
                            implementationByType.Add(implementationType, implementation);
                        }

                        yield return implementation;
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<Type, ILimanImplementation>> GetAllServiceImplementations()
        {
            foreach (var keyValue in implementationsByService.Concat(genericImplementationsByService))
            {
                var serviceType = keyValue.Key;
                var implementations = keyValue.Value;
                foreach (var implementation in implementations)
                {
                    yield return new KeyValuePair<Type, ILimanImplementation>(serviceType, implementation);
                }
            }
        }

        public IEnumerable<ILimanImplementation> GetApplicationImplementations()
        {
            return applicationServices;
        }

        public LimanServiceLifetime GetEffectiveLifetime(ILimanImplementation implementation)
        {
            switch (implementation.Lifetime)
            {
                case LimanServiceLifetime.Any:
                    if (HasScopedDependencies(implementation))
                    {
                        return LimanServiceLifetime.Scoped;
                    }
                    else
                    {
                        return LimanServiceLifetime.Singleton;
                    }
                default:
                    return implementation.Lifetime;
            }
        }

        public void ValidateAll()
        {
            foreach (var implementation in implementationByType.Values)
            {
                Validate(implementation);
            }
        }

        public void Validate(Type serviceType)
        {
            if (TryGetSingle(serviceType, out var implementation))
            {
                Validate(implementation);
            }
            else
            {
                throw new LimanException($"Service '{serviceType}' is not registered.");
            }
        }

        public void Validate(ILimanImplementation implementation)
        {
            switch (implementation.Lifetime)
            {
                case LimanServiceLifetime.Singleton:
                case LimanServiceLifetime.Application:
                    if (HasScopedDependencies(implementation))
                    {
                        var scopedDependencies = string.Join(", ", GetScopedDependencies(implementation));

                        throw new LimanException($"Cannot instantiate singleton '{implementation}' because it has scoped dependencies. ({scopedDependencies})");
                    }
                    break;
            }

            ValidateDependencies(implementation);
        }

        private void ValidateDependencies(ILimanImplementation implementation, Stack<ILimanImplementation>? users = null)
        {
            if (users != null)
            {
                if (users.Contains(implementation))
                {
                    throw new LimanException(ExceptionHelper.CreateCircularDependencyMessage(users, implementation));
                }
            }
            else
            {
                users = new();
            }

            users.Push(implementation);

            foreach (var dependency in implementation.ServiceParameters)
            {
                if (!TryGetSingle(dependency, out var dependencyImplementation))
                {
                    throw new LimanException($"Service '{implementation}' depends on '{dependency}' which is not registered.");
                }

                ValidateDependencies(dependencyImplementation, users);
            }

            if (users.Pop() != implementation) throw new InvalidOperationException();
        }

        private bool HasScopedDependencies(ILimanImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.ServiceParameters)
            {
                if (TryGetSingle(usedServiceType, out var usedImplementationType))
                {
                    switch (usedImplementationType.Lifetime)
                    {
                        case LimanServiceLifetime.Scoped: return true;
                        case LimanServiceLifetime.Any:
                        case LimanServiceLifetime.Transient:
                            if (HasScopedDependencies(usedImplementationType)) return true;
                            break;
                    }
                }
            }

            return false;
        }

        private IEnumerable<ILimanImplementation> GetScopedDependencies(ILimanImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.ServiceParameters)
            {
                if (TryGetSingle(usedServiceType, out var usedImplementationType))
                {
                    switch (usedImplementationType.Lifetime)
                    {
                        case LimanServiceLifetime.Scoped: yield return usedImplementationType; break;
                        case LimanServiceLifetime.Any:
                        case LimanServiceLifetime.Transient:
                            foreach (var decendentScopedImplementationType in GetScopedDependencies(usedImplementationType))
                            {
                                yield return decendentScopedImplementationType;
                            }
                            break;
                    }
                }
            }
        }

        private bool TryAdd(Type implementationType)
        {
            var attribute = implementationType.GetCustomAttribute<LimanServiceAttribute>();
            if (attribute != null)
            {
                Add(implementationType, attribute.Lifetime, attribute.ServiceTypes);
                return true;
            }
            else if (implementationType.IsAssignableTo(typeof(ILimanDependencyConfiguration)))
            {
                var configuration = (ILimanDependencyConfiguration?)Activator.CreateInstance(implementationType)
                    ?? throw new InvalidOperationException();
                configuration.Configure(this);
                return true;
            }
            else if (implementationType.IsAssignableTo(typeof(ILimanClassicDependencyConfiguration)))
            {
                var configuration = (ILimanClassicDependencyConfiguration?)Activator.CreateInstance(implementationType)
                    ?? throw new InvalidOperationException();

                var classicServiceCollection = new ServiceCollection();
                configuration.Configure(classicServiceCollection);
                Add(classicServiceCollection);

                return true;
            }
            else
            {
                return false;
            }
        }

        private static LimanServiceLifetime ToLifetime(ServiceLifetime classicLifetime)
        {
            return classicLifetime switch
            {
                ServiceLifetime.Singleton => LimanServiceLifetime.Singleton,
                ServiceLifetime.Scoped => LimanServiceLifetime.Scoped,
                ServiceLifetime.Transient => LimanServiceLifetime.Transient,
                _ => throw new LimanException($"Classic service lifetime '{classicLifetime}' is not supported"),
            };
        }

        private void Add(LimanImplementation implementation, IEnumerable<Type> serviceTypes)
        {
            implementationByType.Add(implementation.Type, implementation);

            if (implementation.Lifetime == LimanServiceLifetime.Application)
            {
                if (implementation.Type.IsGenericTypeDefinition)
                    throw new LimanException($"Service implementation '{implementation}' cannot have lifetime '{implementation.Lifetime}', because it's a generic type.");
                applicationServices.Add(implementation);
            }

            if (implementation.Type.IsGenericTypeDefinition)
            {
                foreach (var serviceType in serviceTypes)
                {
                    if (!serviceType.IsGenericTypeDefinition) throw new LimanException($"Service implementation '{implementation}' cannot implement service '{serviceType.GetReadableName()}', because the service is not generic");
                    genericImplementationsByService.AddItem(serviceType, implementation);
                }
            }
            else
            {
                foreach (var serviceType in serviceTypes)
                {
                    if (serviceType.IsGenericTypeDefinition) throw new LimanException($"Service implementation '{implementation}' cannot implement service '{serviceType.GetReadableName()}', because the service is generic");
                    implementationsByService.AddItem(serviceType, implementation);
                }
            }
        }
    }
}
