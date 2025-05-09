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
