using Liman.Implementation.Lifetimes;

namespace Liman.Implementation.ServiceFactories
{
    [LimanService(LimanServiceLifetime.Singleton)]
    internal class ServiceFactoryProvider : IServiceFactoryProvider
    {
        private readonly Dictionary<Type, IServiceFactory> factoryByServiceType = [];
        private readonly ILimanServiceCollection serviceImplementationRepository;
        private readonly ILimanServiceLifetimeManager serviceCollection;
        private readonly bool validate;
        private readonly List<ILimanImplementation> creationsInProgress = [];
        private readonly List<ILimanInitializable> uninitialized = [];

        public ServiceFactoryProvider(
            ILimanServiceCollection serviceCollection,
            ILimanServiceLifetimeManager serviceLifetimeManager,
            [NoInjection] bool validate)
        {
            this.serviceImplementationRepository = serviceCollection;
            this.serviceCollection = serviceLifetimeManager;
            this.validate = validate;

            factoryByServiceType.Add(typeof(ServiceFactoryProvider), new ConstantFactory(this));
            factoryByServiceType.Add(serviceCollection.GetType(), new ConstantFactory(serviceCollection));
            factoryByServiceType.Add(serviceLifetimeManager.GetType(), new ConstantFactory(serviceLifetimeManager));
        }

        public IServiceFactory Get(Type serviceType)
        {
            if (factoryByServiceType.TryGetValue(serviceType, out var factory))
            {
                return factory;
            }

            if (serviceImplementationRepository.TryGetSingle(serviceType, out var implementationType))
            {
                if (factoryByServiceType.TryGetValue(implementationType.Type, out factory))
                {
                    return factory;
                }

                factory = Create(implementationType);
                factoryByServiceType.Add(serviceType, factory);
                if (serviceType != implementationType.Type)
                {
                    factoryByServiceType.Add(implementationType.Type, factory);
                }
                return factory;
            }
            else
            {
                return NullServiceFactory.Instance;
            }
        }

        public IEnumerable<IServiceFactory> GetApplicationServices()
        {
            foreach (var implementation in serviceImplementationRepository.GetApplicationImplementations())
            {
                yield return Get(implementation.Type);
            }
        }

        public IServiceFactory[] GetUsedServices(ILimanImplementation parentImplementation)
        {
            var factories = new IServiceFactory[parentImplementation.ServiceParameters.Count];

            int index = 0;

            foreach (var serviceType in parentImplementation.ServiceParameters)
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

        public void PrepareCreation(ILimanImplementation serviceImplementation)
        {
            lock (creationsInProgress)
            {
                if (creationsInProgress.Contains(serviceImplementation))
                {
                    throw new LimanException(ExceptionHelper.CreateCircularDependencyMessage(creationsInProgress, serviceImplementation));
                }

                creationsInProgress.Add(serviceImplementation);
            }
        }

        public void FinishCreation(ILimanImplementation serviceImplementation, object result)
        {
            lock (creationsInProgress)
            {
                if (creationsInProgress[^1] != serviceImplementation) throw new InvalidOperationException();
                creationsInProgress.RemoveAt(creationsInProgress.Count - 1);
            }

            if (result is ILimanInitializable initializable)
            {
                lock (uninitialized)
                {
                    uninitialized.Add(initializable);
                }
            }

            InitializeAll();
        }

        private IServiceFactory Create(ILimanImplementation implementationType)
        {
            if (validate)
            {
                serviceImplementationRepository.Validate(implementationType);
            }

            var effectiveLifetime = serviceImplementationRepository.GetEffectiveLifetime(implementationType);

            return effectiveLifetime switch
            {
                LimanServiceLifetime.Singleton or LimanServiceLifetime.Application => new SingletonServiceFactory(this, serviceCollection, implementationType),
                LimanServiceLifetime.Transient => new TransientServiceFactory(this, serviceCollection, implementationType),
                LimanServiceLifetime.Scoped => new ScopedServiceFactory(this, serviceCollection, implementationType),
                _ => throw new InvalidOperationException(),
            };
        }

        private void InitializeAll()
        {
            do
            {
                List<ILimanInitializable> servicesToInitialize;

                lock (uninitialized)
                {
                    if (creationsInProgress.Count > 0 || uninitialized.Count == 0)
                    {
                        return;
                    }

                    servicesToInitialize = [.. uninitialized];
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
