using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    internal class LimanServiceScope(IServiceProvider serviceProvider) : IServiceScope
    {
        private readonly List<ScopedServiceFactory> registeredFactories = [];

        public IServiceProvider ServiceProvider { get; } = serviceProvider;

        public void Dispose()
        {
            foreach (var factory in registeredFactories)
            {
                factory.Remove(this);
            }

            registeredFactories.Clear();
        }

        internal void RegisterFactory(ScopedServiceFactory factory)
        {
            registeredFactories.Add(factory);
        }
    }
}
