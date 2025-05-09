using Liman.Implementation.Lifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class TransientServiceFactory : ServiceFactoryBase
    {
        private IServiceFactory[]? dependencyFactories;
        private readonly ILimanServiceLifetimeManager serviceLifetimeManager;

        public TransientServiceFactory(
            IServiceFactoryProvider serviceFactoryProvider,
            ILimanServiceLifetimeManager serviceLifetimeManager,
            ILimanServiceImplementation implementationType)
            : base(serviceFactoryProvider, implementationType)
        {
            this.serviceLifetimeManager = serviceLifetimeManager;
        }

        public override LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Transient;

        protected override IServiceFactory[] GetDependencyFactories()
        {
            if (dependencyFactories == null)
            {
                dependencyFactories = base.GetDependencyFactories();
            }

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
