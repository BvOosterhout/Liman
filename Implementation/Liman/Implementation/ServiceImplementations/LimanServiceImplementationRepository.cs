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
            var attribute = implementationType.GetCustomAttribute<ServiceImplementationAttribute>()
                ?? throw new ArgumentException();

            Add(implementationType, attribute.Lifetime, attribute.ServiceTypes);
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

                    if (type.IsAssignableTo(typeof(ILimanDependencyConfiguration)))
                    {
                        var configuration = (ILimanDependencyConfiguration?)Activator.CreateInstance(type) ?? throw new InvalidOperationException();
                        configuration.Configure(this);
                    }
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
            if (attribute == null) return false;

            Add(implementationType, attribute.Lifetime, attribute.ServiceTypes);
            return true;
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
