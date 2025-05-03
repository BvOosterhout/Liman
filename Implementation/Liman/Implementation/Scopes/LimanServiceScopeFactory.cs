using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    [ServiceImplementation(ServiceImplementationLifetime.Transient)]
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
            var scope = new LimanServiceScope();
            var serviceProvider = new LimanServiceProvider(serviceFactoryProvider, serviceLifetimeManager, scope);
            scope.ServiceProvider = serviceProvider;

            return scope;
        }


    }
}
