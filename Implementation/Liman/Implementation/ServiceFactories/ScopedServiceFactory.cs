using Liman.Implementation.Lifetimes;
using Liman.Implementation.Scopes;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class ScopedServiceFactory(
        IServiceFactoryProvider serviceFactoryProvider,
        ILimanServiceLifetimeManager serviceLifetimeManager,
        ILimanServiceImplementation implementationType) : ServiceFactoryBase(serviceFactoryProvider, implementationType)
    {
        private readonly Dictionary<IServiceScope, object> instanceByScope = [];
        private IServiceFactory[]? dependencyFactories;

        public override LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Scoped;

        protected override IServiceFactory[] GetDependencyFactories()
        {
            dependencyFactories ??= base.GetDependencyFactories();

            return dependencyFactories;
        }

        public override object? Get(IServiceScope? scope, object?[] customArguments)
        {
            if (scope == null) throw new LimanException($"Service implementation '{ImplementationType}' cannot be instantiated without a scope");

            if (!instanceByScope.TryGetValue(scope, out var instance))
            {
                instance = CreateInstance(scope, customArguments);

                if (scope is LimanServiceScope limanScope)
                {
                    limanScope.RegisterFactory(this);
                }
            }

            return instance;
        }

        internal object Remove(LimanServiceScope scope)
        {
            if (instanceByScope.Remove(scope, out var instance))
            {
                serviceLifetimeManager.Delete(instance);
                return instance;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        protected override void StoreInstance(object instance, IServiceScope? scope, object?[] customArguments)
        {
            instanceByScope.Add(scope ?? throw new InvalidOperationException(), instance);
        }
    }
}
