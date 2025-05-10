using Liman.Implementation.Lifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class SingletonServiceFactory(
        IServiceFactoryProvider serviceFactoryProvider,
        ILimanServiceLifetimeManager serviceLifetimeManager,
        ILimanServiceImplementation implementationType) : ServiceFactoryBase(serviceFactoryProvider, implementationType)
    {
        private object? instance;

        public override LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Singleton;

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
