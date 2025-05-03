using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Lazies
{
    [ServiceImplementation(ServiceImplementationLifetime.Transient)]
    internal class LazyService<T> : Lazy<T>
        where T : notnull
    {
        public LazyService(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<T>)
        {
        }
    }
}
