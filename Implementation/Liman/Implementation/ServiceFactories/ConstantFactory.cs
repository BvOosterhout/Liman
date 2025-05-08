using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal class ConstantFactory : IServiceFactory
    {
        private readonly object value;

        public ConstantFactory(object value)
        {
            this.value = value;
        }

        public LimanImplementationLifetime Lifetime { get; } = LimanImplementationLifetime.Any;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return value;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
