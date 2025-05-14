using Liman.Implementation.ServiceCollections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation.Classics
{
    internal static class ClassicServiceCollectionHelper
    {
        public static void ApplyTo(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection)
        {
            foreach (var serviceImplementation in limanServiceCollection.GetAllServiceImplementations())
            {
                AddService(limanServiceCollection, classicServiceCollection, serviceImplementation.Key, serviceImplementation.Value);
            }
        }

        private static void AddService(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection, Type serviceType, ILimanImplementation implementation)
        {
            if (implementation.CustomParameters.Count > 0) return;

            var lifetime = ToLifetime(limanServiceCollection, implementation);

            if (implementation.FactoryMethod != null)
            {
                classicServiceCollection.Add(new ServiceDescriptor(serviceType, serviceProvider => CreateClassicInstance(serviceProvider, implementation), lifetime));
            }
            else
            {
                classicServiceCollection.Add(new ServiceDescriptor(serviceType, implementation.Type, lifetime));
            }
        }


        private static ServiceLifetime ToLifetime(ILimanServiceCollection serviceCollection, ILimanImplementation implementation)
        {
            var effectiveLifetime = serviceCollection.GetEffectiveLifetime(implementation);

            return effectiveLifetime switch
            {
                LimanServiceLifetime.Singleton or LimanServiceLifetime.Application => ServiceLifetime.Singleton,
                LimanServiceLifetime.Scoped => ServiceLifetime.Scoped,
                LimanServiceLifetime.Transient => ServiceLifetime.Transient,
                _ => throw new InvalidOperationException($"Lifetime '{effectiveLifetime}' cannot be an effective service lifetime"),
            };
        }

        private static object CreateClassicInstance(IServiceProvider serviceProvider, ILimanImplementation implementation)
        {
            var arguments = implementation.ServiceParameters
                .Select(x => serviceProvider.GetRequiredService(x))
                .ToArray();

            return implementation.CreateInstance(arguments);
        }
    }
}
