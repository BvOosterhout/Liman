using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class ServiceScopeFactory : IServiceFactory
    {
        public LimanImplementationLifetime Lifetime => LimanImplementationLifetime.Transient;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return scope;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
