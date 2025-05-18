using Liman.Implementation.Lifetimes;
using Liman.Implementation.Scopes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Classics
{
    internal class ClassicServiceProviderFactory : IClassicServiceProviderFactory
    {
        private Dictionary<IServiceProvider, LimanServiceProvider> providerByClassic = new();

        public ILimanServiceProvider Get(IServiceProvider classicServiceProvider)
        {
            if (!providerByClassic.TryGetValue(classicServiceProvider, out var result))
            {
                var serviceFactoryProvider = classicServiceProvider.GetRequiredService<IServiceFactoryProvider>();
                var lifetimeManager = classicServiceProvider.GetRequiredService<ILimanServiceLifetimeManager>();

                LimanServiceScope? limanScope;

                try
                {
                    limanScope = classicServiceProvider.GetService<LimanServiceScope>();
                }
                catch
                {
                    // Assume that there is no scope
                    limanScope = null;
                }

                result = new LimanServiceProvider(serviceFactoryProvider, lifetimeManager);
                if (limanScope != null)
                {
                    result.SetScope(limanScope);
                }
            }

            return result;
        }
    }
}
