using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceProviders
{
    [LimanService(LimanServiceLifetime.Transient)]
    internal class LimanServiceProvider(
        IServiceFactoryProvider serviceFactoryProvider,
        ILimanServiceLifetimeManager lifetimeManager) : ILimanServiceProvider, IDependency
    {
        private IServiceScope? scope;
        private object? user;

        public object? GetService(Type serviceType)
        {
            return GetService(serviceType, []);
        }

        public object? GetService(Type serviceType, params object[] customArguments)
        {
            var factory = serviceFactoryProvider.Get(serviceType);
            var implementation = factory.Get(scope, customArguments);

            if (user != null && factory.Lifetime == LimanServiceLifetime.Transient && implementation != null)
            {
                lifetimeManager.AddTransientDependency(user, implementation);
            }

            return implementation;
        }

        public void RemoveService(object service)
        {
            if (user != null)
            {
                lifetimeManager.DeleteTransientDependency(user, service);
            }
            else
            {
                var factory = serviceFactoryProvider.Get(service.GetType());

                if (factory.Lifetime == LimanServiceLifetime.Transient)
                {
                    lifetimeManager.Delete(service);
                }
                else
                {
                    throw new LimanException($"Cannot remove service '{service.GetType().GetReadableName()}', because it is not a transient service.");
                }
            }
        }

        public void RegisterUser(object user)
        {
            if (this.user != null) throw new LimanException("A user was already registered for ServiceProvider");
            this.user = user;
        }

        public void SetScope(IServiceScope scope)
        {
            if (this.scope != null) throw new LimanException("A scope was already set for ServiceProvider");
            this.scope = scope;
        }

        public IEnumerable<object> GetApplicationServices()
        {
            foreach (var factory in serviceFactoryProvider.GetApplicationServices())
            {
                yield return factory.Get(scope, []) ?? throw new InvalidOperationException();
            }
        }

        public void RegisterDependency(object dependency)
        {
            if (user != null)
            {
                lifetimeManager.AddTransientDependency(user, dependency);
            }
            else
            {
                throw new LimanException($"Cannot register dependency '{dependency.GetType().GetReadableName()}', because the service provider is not tied to a user.");
            }
        }

        public void DeregisterDependency(object dependency)
        {
            if (user != null)
            {
                lifetimeManager.DeleteTransientDependency(user, dependency);
            }
            else
            {
                throw new LimanException($"Cannot deregister dependency '{dependency.GetType().GetReadableName()}', because the service provider is not tied to a user.");
            }
        }

        public void RegisterDependency(object user, object dependency)
        {
            lifetimeManager.AddTransientDependency(user, dependency);
        }

        public void DeregisterDependency(object user, object dependency)
        {
            lifetimeManager.DeleteTransientDependency(user, dependency);
        }

        public void Dispose()
        {
            lifetimeManager.DeleteAllServices();
        }
    }
}
