using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class TransientServiceFactory(
        IServiceFactoryProvider serviceFactoryProvider,
        ILimanServiceLifetimeManager serviceLifetimeManager,
        ILimanImplementation implementationType) : ServiceFactoryBase(serviceFactoryProvider, implementationType)
    {
        private IServiceFactory[]? dependencyFactories;

        public override LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Transient;

        protected override IServiceFactory[] GetDependencyFactories()
        {
            dependencyFactories ??= base.GetDependencyFactories();

            return dependencyFactories;
        }

        public override object? Get(IServiceScope? scope, object?[] customArguments)
        {
            var instance = CreateInstance(scope, customArguments);

            if (scope != null && instance is LimanServiceProvider serviceProvider)
            {
                serviceProvider.SetScope(scope);
            }

            return instance;
        }

        public override void RegisterUser(object user, object dependency)
        {
            serviceLifetimeManager.AddTransientDependency(user, dependency);

            if (dependency is LimanServiceProvider serviceProvider)
            {
                serviceProvider.RegisterUser(user);
            }
        }
    }
}
