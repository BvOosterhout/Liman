using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceProviders
{
    [ServiceImplementation(ServiceImplementationLifetime.Singleton)]
    internal class LimanServiceProvider : ILimanServiceProvider, IDependency
    {
        private readonly IServiceFactoryProvider serviceFactoryProvider;
        private readonly ILimanServiceLifetimeManager applicationLifetimeManager;
        private readonly IServiceScope? scope;
        private object? user;

        public LimanServiceProvider(
            IServiceFactoryProvider serviceFactoryProvider,
            ILimanServiceLifetimeManager applicationLifetimeManager,
            IServiceScope? scope)
        {
            this.serviceFactoryProvider = serviceFactoryProvider;
            this.applicationLifetimeManager = applicationLifetimeManager;
            this.scope = scope;
        }

        public object? GetService(Type serviceType)
        {
            return GetService(serviceType, []);
        }

        public object? GetService(Type serviceType, params object[] customArguments)
        {
            var factory = serviceFactoryProvider.Get(serviceType);
            var implementation = factory.Get(scope, customArguments);

            if (user != null && factory.Lifetime == ServiceImplementationLifetime.Transient && implementation != null)
            {
                applicationLifetimeManager.AddTransientDependency(user, implementation);
            }

            return implementation;
        }

        public void RegisterUser(object user)
        {
            if (this.user != null) throw new LimanException("A user was already registered for ServiceProvider");
            this.user = user;
        }

        public IEnumerable<object> GetApplicationServices()
        {
            foreach (var factory in serviceFactoryProvider.GetApplicationServices())
            {
                yield return factory.Get(scope, []) ?? throw new InvalidOperationException();
            }
        }
    }
}
