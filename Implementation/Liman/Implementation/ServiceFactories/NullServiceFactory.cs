using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class NullServiceFactory : IServiceFactory
    {
        public static IServiceFactory Instance { get; } = new NullServiceFactory();

        public LimanImplementationLifetime Lifetime { get; } = LimanImplementationLifetime.Singleton;

        private NullServiceFactory() { }

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return null;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
