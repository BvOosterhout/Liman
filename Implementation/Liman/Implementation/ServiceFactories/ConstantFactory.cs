using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class ConstantFactory(object value) : IServiceFactory
    {
        public LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Any;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return value;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
