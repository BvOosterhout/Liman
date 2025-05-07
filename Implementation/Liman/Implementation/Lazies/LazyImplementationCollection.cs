using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Liman.Implementation.Lazies
{
    [ServiceImplementation(ServiceImplementationLifetime.Transient, typeof(LazyImplementationCollection<>), typeof(ILazyImplementationCollection<>))]
    internal class LazyImplementationCollection<T> : ILazyImplementationCollection<T>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILimanServiceCollection serviceCollection;
        private List<T>? implementations;

        public LazyImplementationCollection(IServiceProvider serviceProvider, ILimanServiceCollection serviceCollection)
        {
            this.serviceProvider = serviceProvider;
            this.serviceCollection = serviceCollection;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetImplementations().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetImplementations().GetEnumerator();
        }

        private List<T> GetImplementations()
        {
            if (implementations == null)
            {
                var implementationTypes = serviceCollection.GetAll(typeof(T));

                implementations = new List<T>();

                foreach (var type in implementationTypes)
                {
                    implementations.Add((T?)serviceProvider.GetRequiredService(type.Type) ?? throw new InvalidOperationException());
                }
            }

            return implementations;
        }
    }
}
