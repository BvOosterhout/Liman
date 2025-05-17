using Liman.Implementation.Lifetimes;
using Liman.Implementation.Scopes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation.Classics
{
    internal class ClassicServiceProviderFactory : IClassicServiceProviderFactory
    {
        private Dictionary<IServiceProvider, ILimanServiceProvider> providerByClassic = new();

        public ILimanServiceProvider Get(IServiceProvider classicServiceProvider)
        {
            if (!providerByClassic.TryGetValue(classicServiceProvider, out var result))
            {
                var serviceFactoryProvider = classicServiceProvider.GetRequiredService<IServiceFactoryProvider>();
                var lifetimeManager = classicServiceProvider.GetRequiredService<ILimanServiceLifetimeManager>();

                result = new LimanServiceProvider(serviceFactoryProvider, lifetimeManager);
                (result as LimanServiceProvider)!.SetScope(new LimanServiceScope(classicServiceProvider));
            }

            return result;
        }
    }
}
