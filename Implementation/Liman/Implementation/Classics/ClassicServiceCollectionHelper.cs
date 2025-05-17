using Liman.Implementation.Lifetimes;
using Liman.Implementation.Scopes;
using Liman.Implementation.ServiceCollections;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Liman.Implementation.Classics
{
    internal static class ClassicServiceCollectionHelper
    {
        private static readonly Assembly limanAssembly = Assembly.GetExecutingAssembly();

        public static void ApplyTo(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection)
        {
            foreach (var serviceImplementation in limanServiceCollection.GetAllServiceImplementations())
            {
                AddService(limanServiceCollection, classicServiceCollection, serviceImplementation.Key, serviceImplementation.Value);
            }
        }

        private static void AddService(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection, Type serviceType, ILimanImplementation implementation)
        {

            if (implementation.Type.Assembly == limanAssembly)
            {
                // Some Liman services need special treatment
                AddLimanService(limanServiceCollection, classicServiceCollection, serviceType, implementation);
            }
            else
            {
                AddRegularService(limanServiceCollection, classicServiceCollection, serviceType, implementation);
            }
        }

        private static void AddRegularService(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection, Type serviceType, ILimanImplementation implementation)
        {
            if (implementation.CustomParameters.Count > 0) return;

            var lifetime = ToLifetime(limanServiceCollection, implementation);

            if (serviceType.IsGenericTypeDefinition)
            {
                if (implementation.FactoryMethod != null)
                {
                    throw new LimanException($"Cannot add service '{serviceType.GetReadableName()}' because generic types with a factory method are not supported in a classic service collection.");
                }

                if (implementation.Type.GetInterfaces().Contains(typeof(ILimanInitializable)))
                {
                    throw new LimanException($"Cannot add service '{serviceType.GetReadableName()}' because generic types with an initializer method are not supported in a classic service collection.");
                }

                classicServiceCollection.Add(new ServiceDescriptor(serviceType, implementation.Type, lifetime));
            }
            else
            {
                classicServiceCollection.Add(new ServiceDescriptor(serviceType, serviceProvider => CreateClassicInstance(serviceProvider, implementation), lifetime));
            }
        }

        private static void AddLimanService(ILimanServiceCollection limanServiceCollection, IServiceCollection classicServiceCollection, Type serviceType, ILimanImplementation implementation)
        {
            if (serviceType == typeof(Lazy<>))
            {
                if (!classicServiceCollection.Any(x => x.ServiceType == typeof(Lazy<>)))
                {
                    // Only add if there isn't already an implementation
                    AddRegularService(limanServiceCollection, classicServiceCollection, serviceType, implementation);
                }
            }
            else if (implementation.Type == typeof(LimanServiceCollection))
            {
                classicServiceCollection.AddSingleton(serviceType, limanServiceCollection);
            }
            else if (implementation.Type == typeof(LimanServiceProvider))
            {
                if (serviceType == typeof(ILimanServiceProvider))
                {
                    classicServiceCollection.AddScoped<ILimanServiceProvider>(classicServiceProvider => {
                        var serviceFactoryProvider = classicServiceProvider.GetRequiredService<IServiceFactoryProvider>();
                        var lifetimeManager = classicServiceProvider.GetRequiredService<ILimanServiceLifetimeManager>();

                        var result = new LimanServiceProvider(serviceFactoryProvider, lifetimeManager);
                        result.SetScope(new LimanServiceScope(classicServiceProvider));
                        return result;
                    });
                }
            }
            else if (implementation.Type == typeof(ServiceFactoryProvider))
            {
                if (serviceType == typeof(IServiceFactoryProvider))
                {
                    classicServiceCollection.AddSingleton<IServiceFactoryProvider>(classicServiceProvider => {
                        var lifetimeManager = classicServiceProvider.GetRequiredService<ILimanServiceLifetimeManager>();
                        return new ServiceFactoryProvider(limanServiceCollection, lifetimeManager, x => new ClassicServiceFactory(classicServiceProvider, x), validate: true);
                    });
                }
            }
            else if (implementation.Type == typeof(LimanServiceScopeFactory))
            {
                if (serviceType == typeof(IServiceScopeFactory) && !classicServiceCollection.Any(x => x.ServiceType == typeof(IServiceScopeFactory)))
                {
                    // Only add if there isn't already an implementation
                    AddRegularService(limanServiceCollection, classicServiceCollection, serviceType, implementation);
                }
            }
            else if (implementation.Type == typeof(LimanServiceLifetimeManager))
            {
                if (serviceType == implementation.Type)
                {
                    classicServiceCollection.AddSingleton<LimanServiceLifetimeManager>();
                }
                else
                {
                    classicServiceCollection.AddSingleton(serviceType, x => x.GetRequiredService<LimanServiceLifetimeManager>());
                }
            }
            else
            {
                AddRegularService(limanServiceCollection, classicServiceCollection, serviceType, implementation);
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

        private static object CreateClassicInstance(IServiceProvider classicServiceProvider, ILimanImplementation implementation)
        {
            var limanServiceProvider = classicServiceProvider.GetRequiredService<ILimanServiceProvider>();
            return limanServiceProvider.GetRequiredService(implementation.Type);
        }
    }
}
