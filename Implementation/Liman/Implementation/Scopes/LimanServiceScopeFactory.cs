using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    [LimanImplementation(LimanImplementationLifetime.Transient)]
    internal class LimanServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceFactoryProvider serviceFactoryProvider;
        private readonly ILimanServiceLifetimeManager serviceLifetimeManager;

        public LimanServiceScopeFactory(IServiceFactoryProvider serviceFactoryProvider, ILimanServiceLifetimeManager serviceLifetimeManager)
        {
            this.serviceFactoryProvider = serviceFactoryProvider;
            this.serviceLifetimeManager = serviceLifetimeManager;
        }

        public IServiceScope CreateScope()
        {
            var serviceProvider = new LimanServiceProvider(serviceFactoryProvider, serviceLifetimeManager);
            var scope = new LimanServiceScope(serviceProvider);
            serviceProvider.SetScope(scope);

            return scope;
        }
    }
}
