using Liman.Implementation.Lifetimes;

namespace Liman.Implementation.ServiceFactories
{
    [LimanService(LimanServiceLifetime.Singleton)]
    internal class ServiceFactoryProvider : IServiceFactoryProvider
    {
        private readonly Dictionary<Type, IServiceFactory> factoryByServiceType = [];
        private readonly Dictionary<ILimanImplementation, IServiceFactory> factoryByImplementationType = [];
        private readonly ILimanServiceCollection serviceCollection;
        private readonly ILimanServiceLifetimeManager lifetimeManager;
        private readonly Func<Type, IServiceFactory> defaultServiceFactoryBuilder;
        private readonly bool validate;
        private readonly List<ILimanImplementation> creationsInProgress = [];
        private readonly List<ILimanInitializable> uninitialized = [];

        public ServiceFactoryProvider(
            ILimanServiceCollection serviceCollection,
            ILimanServiceLifetimeManager serviceLifetimeManager,
            [NoInjection] Func<Type, IServiceFactory> defaultServiceFactoryBuilder,
            [NoInjection] bool validate)
        {
            this.serviceCollection = serviceCollection;
            this.lifetimeManager = serviceLifetimeManager;
            this.defaultServiceFactoryBuilder = defaultServiceFactoryBuilder;
            this.validate = validate;

            AddConstant(this);
            AddConstant(serviceCollection);
            AddConstant(serviceLifetimeManager);
        }

        public IServiceFactory Get(Type serviceType)
        {
            if (factoryByServiceType.TryGetValue(serviceType, out var factory))
            {
                return factory;
            }

            if (serviceCollection.TryGetSingle(serviceType, out var implementationType))
            {
                factory = Get(implementationType);
                factoryByServiceType.Add(serviceType, factory);
                return factory;
            }
            else
            {
                factory = defaultServiceFactoryBuilder.Invoke(serviceType);
                factoryByServiceType.Add(serviceType, factory);
                return factory;
            }
        }

        public IServiceFactory Get(ILimanImplementation implementationType)
        {
            if (!factoryByImplementationType.TryGetValue(implementationType, out var factory))
            {
                factory = Create(implementationType);
                factoryByImplementationType.Add(implementationType, factory);
            }

            return factory;
        }

        public IEnumerable<IServiceFactory> GetApplicationServices()
        {
            foreach (var implementation in serviceCollection.GetApplicationImplementations())
            {
                yield return Get(implementation);
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
                    if (parentImplementation.Type == null)
                    {
                        throw new LimanException($"Cannot fill the 'Type' parameter for the constructor of {parentImplementation}, no type was defined.");
                    }

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

        private void AddConstant(object value)
        {
            if (!serviceCollection.TryGetSingle(value.GetType(), out var implementationType))
            {
                throw new ArgumentException();
            }

            factoryByImplementationType.Add(implementationType, new ConstantFactory(value));
        }

        private IServiceFactory Create(ILimanImplementation implementationType)
        {
            if (validate)
            {
                serviceCollection.Validate(implementationType);
            }

            var effectiveLifetime = serviceCollection.GetEffectiveLifetime(implementationType);

            return effectiveLifetime switch
            {
                LimanServiceLifetime.Singleton or LimanServiceLifetime.Application => new SingletonServiceFactory(this, lifetimeManager, implementationType),
                LimanServiceLifetime.Transient => new TransientServiceFactory(this, lifetimeManager, implementationType),
                LimanServiceLifetime.Scoped => new ScopedServiceFactory(this, lifetimeManager, implementationType),
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
