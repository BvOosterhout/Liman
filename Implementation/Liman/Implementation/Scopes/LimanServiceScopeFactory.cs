using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    [LimanService(LimanServiceLifetime.Transient)]
    internal class LimanServiceScopeFactory(IServiceFactoryProvider serviceFactoryProvider, ILimanServiceLifetimeManager serviceLifetimeManager) : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            var serviceProvider = new LimanServiceProvider(serviceFactoryProvider, serviceLifetimeManager);
            var scope = new LimanServiceScope(serviceProvider);
            serviceProvider.SetScope(scope);

            return scope;
        }
    }
}
