using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Scopes
{
    internal class LimanServiceScope : IServiceScope, IDisposable
    {
        private List<ScopedServiceFactory> registeredFactories = new();

        public IServiceProvider ServiceProvider { get; internal set; }

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
