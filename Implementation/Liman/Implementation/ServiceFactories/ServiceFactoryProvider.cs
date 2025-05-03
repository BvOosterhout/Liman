using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceImplementations;

namespace Liman.Implementation.ServiceFactories
{
    internal class ServiceFactoryProvider : IServiceFactoryProvider
    {
        private readonly Dictionary<Type, IServiceFactory> factoryByServiceType = new();
        private readonly ILimanServiceImplementationRepository serviceImplementationRepository;
        private readonly ILimanServiceLifetimeManager serviceLifetimeManager;
        private readonly List<LimanServiceImplementation> creationsInProgress = new();
        private readonly List<IInitializable> uninitialized = new();

        public ServiceFactoryProvider(
            ILimanServiceImplementationRepository serviceImplementationRepository,
            ILimanServiceLifetimeManager serviceLifetimeManager)
        {
            this.serviceImplementationRepository = serviceImplementationRepository;
            this.serviceLifetimeManager = serviceLifetimeManager;
        }

        public IServiceFactory Get(Type serviceType)
        {
            if (factoryByServiceType.TryGetValue(serviceType, out var factory))
            {
                return factory;
            }

            if (serviceImplementationRepository.TryGet(serviceType, out var implementationType))
            {
                return Create(implementationType);
            }
            else
            {
                return NullServiceFactory.Instance;
            }
        }

        public IEnumerable<IServiceFactory> GetApplicationServices()
        {
            var implementations = serviceImplementationRepository.GetApplicationImplementations();

            foreach (var implementation in serviceImplementationRepository.GetApplicationImplementations())
            {
                yield return Get(implementation.Type);
            }
        }

        public IServiceFactory[] GetUsedServices(LimanServiceImplementation parentImplementation)
        {
            var factories = new IServiceFactory[parentImplementation.UsedServices.Count];

            int index = 0;

            foreach (var serviceType in parentImplementation.UsedServices)
            {
                IServiceFactory factory;

                if (serviceType == typeof(Type))
                {
                    factory = new ConstantFactory(parentImplementation.Type);
                }
                else
                {
                    factory = Get(serviceType);
                }

                factories[index] = factory;
                index++;
            }

            return factories;
        }

        public void PrepareCreation(LimanServiceImplementation serviceImplementation)
        {
            lock (creationsInProgress)
            {
                if (creationsInProgress.Contains(serviceImplementation))
                {
                    throw new CircularDependencyException(creationsInProgress, serviceImplementation);
                }

                creationsInProgress.Add(serviceImplementation);
            }
        }

        public void FinishCreation(LimanServiceImplementation serviceImplementation, object result)
        {
            lock (creationsInProgress)
            {
                if (creationsInProgress[creationsInProgress.Count - 1] != serviceImplementation) throw new ArgumentException();
                creationsInProgress.RemoveAt(creationsInProgress.Count - 1);
            }

            if (result is IInitializable initializable)
            {
                lock (uninitialized)
                {
                    uninitialized.Add(initializable);
                }
            }

            InitializeAll();
        }

        private IServiceFactory Create(LimanServiceImplementation implementationType)
        {
            switch (implementationType.Lifetime)
            {
                case ServiceImplementationLifetime.Singleton:
                case ServiceImplementationLifetime.Application:
                    if (HasScopedDependencies(implementationType))
                    {
                        var scopedDependencies = string.Join(", ", GetScopedDependencies(implementationType));

                        throw new InvalidServiceLifetimeException($"Cannot instantiate singleton '{implementationType}' because it has scoped dependencies. ({scopedDependencies})");
                    }
                    return new SingletonServiceFactory(this, serviceLifetimeManager, implementationType);
                case ServiceImplementationLifetime.Transient:
                    return new TransientServiceFactory(this, serviceLifetimeManager, implementationType);
                case ServiceImplementationLifetime.Scoped:
                    return new ScopedServiceFactory(this, serviceLifetimeManager, implementationType);
                case ServiceImplementationLifetime.Any:
                    if (HasScopedDependencies(implementationType))
                    {
                        return new ScopedServiceFactory(this, serviceLifetimeManager, implementationType);
                    }
                    else
                    {
                        return new SingletonServiceFactory(this, serviceLifetimeManager, implementationType);
                    }
                default:
                    throw new NotSupportedException($"Lifetime '{implementationType.Lifetime}' is not supported");
            }
        }

        private bool HasScopedDependencies(LimanServiceImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.UsedServices)
            {
                if (serviceImplementationRepository.TryGet(usedServiceType, out var usedImplementationType))
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

        private IEnumerable<LimanServiceImplementation> GetScopedDependencies(LimanServiceImplementation implementationType)
        {
            foreach (var usedServiceType in implementationType.UsedServices)
            {
                if (serviceImplementationRepository.TryGet(usedServiceType, out var usedImplementationType))
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

        private void InitializeAll()
        {
            do
            {
                List<IInitializable> servicesToInitialize;

                lock (uninitialized)
                {
                    if (creationsInProgress.Count > 0 || uninitialized.Count == 0)
                    {
                        return;
                    }

                    servicesToInitialize = new List<IInitializable>(uninitialized);
                    uninitialized.Clear();
                }

                foreach (var service in servicesToInitialize)
                {
                    service.Initialize();
                }
            } while (true);
        }
    }
}
