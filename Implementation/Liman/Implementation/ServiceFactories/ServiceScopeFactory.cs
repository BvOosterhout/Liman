using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class ServiceScopeFactory : IServiceFactory
    {
        public LimanServiceLifetime Lifetime => LimanServiceLifetime.Transient;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return scope;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
