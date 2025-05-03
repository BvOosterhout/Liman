using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceImplementations;
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
            LimanServiceImplementation implementationType)
            : base(serviceFactoryProvider, implementationType)
        {
            this.serviceLifetimeManager = serviceLifetimeManager;
        }

        public override ServiceImplementationLifetime Lifetime { get; } = ServiceImplementationLifetime.Transient;

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
