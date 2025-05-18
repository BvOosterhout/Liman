using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Liman.Implementation.Lazies
{
    [LimanService(LimanServiceLifetime.Transient, typeof(LazyImplementationCollection<>), typeof(ILimanImplementationCollection<>))]
    internal class LazyImplementationCollection<T> : ILimanImplementationCollection<T>
    {
        private readonly ILimanServiceProvider serviceProvider;
        private readonly ILimanServiceCollection serviceCollection;
        private List<T>? implementations;

        public LazyImplementationCollection(ILimanServiceProvider serviceProvider, ILimanServiceCollection serviceCollection)
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

                foreach (var implementationType in implementationTypes)
                {
                    implementations.Add((T?)serviceProvider.GetRequiredService(implementationType) ?? throw new InvalidOperationException());
                }
            }

            return implementations;
        }
    }
}
