using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    internal class LimanServiceScope : IServiceScope
    {
        private List<ScopedServiceFactory> registeredFactories = new();

        public LimanServiceScope(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            foreach (var factory in registeredFactories)
            {
                var instance = factory.Remove(this);
            }

            registeredFactories.Clear();
        }

        internal void RegisterFactory(ScopedServiceFactory factory)
        {
            registeredFactories.Add(factory);
        }
    }
}
