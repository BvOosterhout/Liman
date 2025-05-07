using Liman.Implementation.Lifetimes;
using Liman.Implementation.Scopes;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    [ServiceImplementation(ServiceImplementationLifetime.Singleton)]
    internal class ServiceFactoryProvider : IServiceFactoryProvider
    {
        private readonly Dictionary<Type, IServiceFactory> factoryByServiceType = new();
        private readonly ILimanServiceCollection serviceImplementationRepository;
        private readonly ILimanServiceLifetimeManager serviceCollection;
        private readonly bool validate;
        private readonly List<ILimanServiceImplementation> creationsInProgress = new();
        private readonly List<IInitializable> uninitialized = new();

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
            factoryByServiceType.Add(typeof(LimanServiceScope), new ServiceScopeFactory());
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
            var implementations = serviceImplementationRepository.GetApplicationImplementations();

            foreach (var implementation in serviceImplementationRepository.GetApplicationImplementations())
            {
                yield return Get(implementation.Type);
            }
        }

        public IServiceFactory[] GetUsedServices(ILimanServiceImplementation parentImplementation)
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

        public void PrepareCreation(ILimanServiceImplementation serviceImplementation)
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

        public void FinishCreation(ILimanServiceImplementation serviceImplementation, object result)
        {
            lock (creationsInProgress)
            {
                if (creationsInProgress[creationsInProgress.Count - 1] != serviceImplementation) throw new InvalidOperationException();
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

        private IServiceFactory Create(ILimanServiceImplementation implementationType)
        {
            if (validate)
            {
                serviceImplementationRepository.Validate(implementationType);
            }

            var effectiveLifetime = serviceImplementationRepository.GetEffectiveLifetime(implementationType);

            switch (effectiveLifetime)
            {
                case ServiceImplementationLifetime.Singleton:
                case ServiceImplementationLifetime.Application:
                    return new SingletonServiceFactory(this, serviceCollection, implementationType);
                case ServiceImplementationLifetime.Transient:
                    return new TransientServiceFactory(this, serviceCollection, implementationType);
                case ServiceImplementationLifetime.Scoped:
                    return new ScopedServiceFactory(this, serviceCollection, implementationType);
                default:
                    throw new InvalidOperationException();
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
