using Liman.Implementation;
using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceCollections;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceProviders;

namespace Liman
{
    public static class LimanFactory
    {
        public static ILimanServiceCollection CreateServiceCollection()
        {
            return new LimanServiceCollection();
        }

        public static ILimanServiceProvider CreateServiceProvider(this ILimanServiceCollection serviceCollection, bool validate = true)
        {
            if (serviceCollection is LimanServiceCollection implementationRepository)
            {
                var lifetimeManager = new LimanServiceLifetimeManager(implementationRepository);
                var serviceFactory = new ServiceFactoryProvider(implementationRepository, lifetimeManager, x => NullServiceFactory.Instance, validate);
                var serviceProvider = new LimanServiceProvider(serviceFactory, lifetimeManager);
                return serviceProvider;
            }
            else
            {
                throw new LimanException($"Service collection of type '{serviceCollection.GetType().GetReadableName()}' is not supported");
            }
        }

        public static ILimanApplication CreateApplication(this ILimanServiceCollection serviceCollection)
        {
            var serviceProvider = CreateServiceProvider(serviceCollection);

            return new LimanApplication(serviceProvider);
        }
    }
}
