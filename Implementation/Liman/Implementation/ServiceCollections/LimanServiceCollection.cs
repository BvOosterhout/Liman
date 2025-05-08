using Liman;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Liman.Implementation.ServiceCollections
{
    [ServiceImplementation(ServiceImplementationLifetime.Singleton)]
    internal class LimanServiceCollection : ILimanServiceCollection
    {
        private Dictionary<Type, List<LimanServiceImplementation>> implementationsByService = new();
        private Dictionary<Type, List<LimanServiceImplementation>> genericImplementationsByService = new();
        private Dictionary<Type, LimanServiceImplementation> implementationByType = new();
        private List<LimanServiceImplementation> applicationServices = new();
        private List<Assembly> addedAssemblies = new();

        public LimanServiceCollection()
        {
            Add(Assembly.GetExecutingAssembly());
        }

        public void Add(Type implementationType)
        {
            if (!TryAdd(implementationType))
            {
                throw new LimanException($"Type '{implementationType.GetReadableName()}' is missing {nameof(ServiceImplementationAttribute)}");
            }
        }

        public void Add(Type implementationType, ServiceImplementationLifetime lifetime, Delegate? constructor = null)
        {
            Add(implementationType, lifetime, Enumerable.Empty<Type>(), constructor);
        }

        public void Add(Type implementationType, ServiceImplementationLifetime lifetime, IEnumerable<Type> serviceTypes, Delegate? constructor = null)
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

            var implementation = new LimanServiceImplementation(implementationType, lifetime, constructor);

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

                implementation = new LimanServiceImplementation(implementationType, ToLifetime(service.Lifetime), factoryMethod);
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

        public bool TryGetSingle(Type serviceType, [MaybeNullWhen(false)] out ILimanServiceImplementation serviceImplementation)
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

        public IEnumerable<ILimanServiceImplementation> GetAll(Type serviceType)
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
                            implementation = new LimanServiceImplementation(implementationType, genericImplementation.Lifetime, null);
                            implementationByType.Add(implementationType, implementation);
                        }

                        yield return implementation;
                    }
                }
            }
        }

        public IEnumerable<ILimanServiceImplementation> GetApplicationImplementations()
        {
            return applicationServices;
        }

        public void ApplyTo(IServiceCollection serviceCollection)
        {
            foreach (var keyValue in implementationsByService)
            {
                var serviceType = keyValue.Key;
                var implementations = keyValue.Value;

                foreach (var implementation in implementations)
                {
                    ApplyTo(serviceCollection, serviceType, implementation);
                }
            }
        }

        public ServiceImplementationLifetime GetEffectiveLifetime(ILimanServiceImplementation implementation)
        {
            switch (implementation.Lifetime)
            {
                case ServiceImplementationLifetime.Any:
                    if (HasScopedDependencies(implementation))
                    {
                        return ServiceImplementationLifetime.Scoped;
                    }
                    else
                    {
                        return ServiceImplementationLifetime.Singleton;
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

        public void Validate(ILimanServiceImplementation implementation)
        {
            switch (implementation.Lifetime)
            {
                case ServiceImplementationLifetime.Singleton:
                case ServiceImplementationLifetime.Application:
                    if (HasScopedDependencies(implementation))
                    {
                        var scopedDependencies = string.Join(", ", GetScopedDependencies(implementation));

                        throw new LimanException($"Cannot instantiate singleton '{implementation}' because it has scoped dependencies. ({scopedDependencies})");
                    }
                    break;
            }

            ValidateDependencies(implementation);
        }


        private void ApplyTo(IServiceCollection serviceCollection, Type serviceType, LimanServiceImplementation implementation)
        {
            if (implementation.CustomParameters.Count > 0) return;

            if (implementation.FactoryMethod.IsConstructor)
            {
                serviceCollection.Add(new ServiceDescriptor(serviceType, implementation.Type, ToLifetime(implementation)));
            }
            else
            {
                serviceCollection.Add(new ServiceDescriptor(serviceType, serviceProvider => CreateClassicInstance(serviceProvider, implementation), ToLifetime(implementation)));
            }
        }

        private object CreateClassicInstance(IServiceProvider serviceProvider, LimanServiceImplementation implementation)
        {
            var arguments = implementation.ServiceParameters
                .Select(x => serviceProvider.GetRequiredService(x))
                .ToArray();

            return implementation.CreateInstance(arguments);
        }

        private void ValidateDependencies(ILimanServiceImplementation implementation, Stack<ILimanServiceImplementation>? users = null)
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

        private bool HasScopedDependencies(ILimanServiceImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.ServiceParameters)
            {
                if (TryGetSingle(usedServiceType, out var usedImplementationType))
                {
                    switch (usedImplementationType.Lifetime)
                    {
                        case ServiceImplementationLifetime.Scoped: return true;
                        case ServiceImplementationLifetime.Any:
                        case ServiceImplementationLifetime.Transient:
                            if (HasScopedDependencies(usedImplementationType)) return true;
                            break;
                    }
                }
            }

            return false;
        }

        private IEnumerable<ILimanServiceImplementation> GetScopedDependencies(ILimanServiceImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.ServiceParameters)
            {
                if (TryGetSingle(usedServiceType, out var usedImplementationType))
                {
                    switch (usedImplementationType.Lifetime)
                    {
                        case ServiceImplementationLifetime.Scoped: yield return usedImplementationType; break;
                        case ServiceImplementationLifetime.Any:
                        case ServiceImplementationLifetime.Transient:
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
            var attribute = implementationType.GetCustomAttribute<ServiceImplementationAttribute>();
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

        private ServiceImplementationLifetime ToLifetime(ServiceLifetime classicLifetime)
        {
            switch (classicLifetime)
            {
                case ServiceLifetime.Singleton: return ServiceImplementationLifetime.Singleton;
                case ServiceLifetime.Scoped: return ServiceImplementationLifetime.Scoped;
                case ServiceLifetime.Transient: return ServiceImplementationLifetime.Transient;
                default: throw new LimanException($"Classic service lifetime '{classicLifetime}' is not supported");
            }
        }

        private ServiceLifetime ToLifetime(LimanServiceImplementation implementation)
        {
            var effectiveLifetime = GetEffectiveLifetime(implementation);

            switch (effectiveLifetime)
            {
                case ServiceImplementationLifetime.Singleton:
                case ServiceImplementationLifetime.Application:
                    return ServiceLifetime.Singleton;
                case ServiceImplementationLifetime.Scoped:
                    return ServiceLifetime.Scoped;
                case ServiceImplementationLifetime.Transient:
                    return ServiceLifetime.Transient;
                default: throw new InvalidOperationException($"Lifetime '{effectiveLifetime}' cannot be an effective service lifetime");
            }
        }

        private void Add(LimanServiceImplementation implementation, IEnumerable<Type> serviceTypes)
        {
            implementationByType.Add(implementation.Type, implementation);

            if (implementation.Lifetime == ServiceImplementationLifetime.Application)
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
