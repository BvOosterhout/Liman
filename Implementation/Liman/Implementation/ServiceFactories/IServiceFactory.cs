using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal interface IServiceFactory
    {
        public ServiceImplementationLifetime Lifetime { get; }

        public object? Get(IServiceScope? scope, object?[] customArguments);

        public void RegisterUser(object user, object dependency);
    }
}
