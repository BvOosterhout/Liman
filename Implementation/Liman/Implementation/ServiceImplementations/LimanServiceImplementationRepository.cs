using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Liman.Implementation.ServiceImplementations
{
    [ServiceImplementation(ServiceImplementationLifetime.Singleton)]
    internal class LimanServiceImplementationRepository : ILimanServiceImplementationRepository
    {
        private Dictionary<Type, List<LimanServiceImplementation>> implementationsByService = new();
        private Dictionary<Type, List<LimanServiceImplementation>> genericImplementationsByService = new();
        private Dictionary<Type, LimanServiceImplementation> implementationByType = new();
        private List<LimanServiceImplementation> applicationServices = new();

        public void Add(Type implementationType)
        {
            if (!TryAdd(implementationType))
            {
                throw new ArgumentException($"Type '{implementationType.GetReadableName()}' is missing {nameof(ServiceImplementationAttribute)}");
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
                throw new NotSupportedException("Keyed services are not supported.");
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
                if (!service.ServiceType.IsGenericTypeDefinition) throw new ArgumentException();
                genericImplementationsByService.AddItem(service.ServiceType, implementation);
            }
            else
            {
                if (service.ServiceType.IsGenericTypeDefinition) throw new ArgumentException();
                implementationsByService.AddItem(service.ServiceType, implementation);
            }
        }

        public void Add(Assembly assembly, params Type[] exceptions)
        {
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

        public bool TryGet(Type serviceType, [MaybeNullWhen(false)] out LimanServiceImplementation serviceImplementation)
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
                throw new ArgumentException();
            }
        }

        public IEnumerable<LimanServiceImplementation> GetAll(Type serviceType)
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
                        var implementation = new LimanServiceImplementation(implementationType, genericImplementation.Lifetime, null);
                        yield return implementation;
                    }
                }
            }
        }

        public void ApplyTo(IServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LimanServiceImplementation> GetApplicationImplementations()
        {
            return applicationServices;
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
                default: throw new NotSupportedException($"Classic service lifetime '{classicLifetime}' is not supported");
            }
        }

        private void Add(LimanServiceImplementation implementation, IEnumerable<Type> serviceTypes)
        {
            implementationByType.Add(implementation.Type, implementation);

            if (implementation.Lifetime == ServiceImplementationLifetime.Application)
            {
                if (implementation.Type.IsGenericTypeDefinition) throw new ArgumentException();
                applicationServices.Add(implementation);
            }

            if (implementation.Type.IsGenericTypeDefinition)
            {
                foreach (var serviceType in serviceTypes)
                {
                    if (!serviceType.IsGenericTypeDefinition) throw new ArgumentException();
                    genericImplementationsByService.AddItem(serviceType, implementation);
                }
            }
            else
            {
                foreach (var serviceType in serviceTypes)
                {
                    if (serviceType.IsGenericTypeDefinition) throw new ArgumentException();
                    implementationsByService.AddItem(serviceType, implementation);
                }
            }
        }
    }
}
