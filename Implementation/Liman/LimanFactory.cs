using Liman.Implementation;
using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Liman.Implementation.ServiceImplementations;
using Liman.Implementation.ServiceProviders;

namespace Liman
{
    public static class LimanFactory
    {
        public static ILimanServiceCollection CreateServiceCollection()
        {
            return new LimanServiceImplementationRepository();
        }

        public static ILimanServiceProvider CreateServiceProvider(ILimanServiceCollection serviceCollection, bool validate = true)
        {
            if (serviceCollection is LimanServiceImplementationRepository implementationRepository)
            {
                var lifetimeManager = new LimanServiceLifetimeManager(implementationRepository);
                var serviceFactory = new ServiceFactoryProvider(implementationRepository, lifetimeManager, validate);
                var serviceProvider = new LimanServiceProvider(serviceFactory, lifetimeManager, scope: null);
                return serviceProvider;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static ILimanApplication CreateApplication(ILimanServiceCollection serviceCollection)
        {
            var serviceProvider = CreateServiceProvider(serviceCollection);

            return new LimanApplication(serviceProvider);
        }
    }
}
