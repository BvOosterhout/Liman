using Liman.Implementation.Lifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class TransientServiceFactory(
        IServiceFactoryProvider serviceFactoryProvider,
        ILimanServiceLifetimeManager serviceLifetimeManager,
        ILimanServiceImplementation implementationType) : ServiceFactoryBase(serviceFactoryProvider, implementationType)
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
            return CreateInstance(scope, customArguments);
        }

        public override void RegisterUser(object user, object dependencyObject)
        {
            serviceLifetimeManager.AddTransientDependency(user, dependencyObject);

            if (dependencyObject is IDependency dependency)
            {
                dependency.RegisterUser(user);
            }
        }
    }
}
