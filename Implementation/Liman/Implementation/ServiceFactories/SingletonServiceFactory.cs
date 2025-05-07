using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceImplementations;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class SingletonServiceFactory : ServiceFactoryBase
    {
        private readonly ILimanServiceLifetimeManager serviceLifetimeManager;
        private object? instance;

        public SingletonServiceFactory(
            IServiceFactoryProvider serviceFactoryProvider,
            ILimanServiceLifetimeManager serviceLifetimeManager,
            ILimanServiceImplementation implementationType)
            : base(serviceFactoryProvider, implementationType)
        {
            this.serviceLifetimeManager = serviceLifetimeManager;
        }

        public override ServiceImplementationLifetime Lifetime { get; } = ServiceImplementationLifetime.Singleton;

        public override object? Get(IServiceScope? scope, object?[] customArguments)
        {
            if (instance == null)
            {
                instance = CreateInstance(scope, customArguments);
                serviceLifetimeManager.AddSingleton(instance);
            }

            return instance;
        }

        protected override void StoreInstance(object instance, IServiceScope? scope, object?[] customArguments)
        {
            this.instance = instance;
        }
    }
}
