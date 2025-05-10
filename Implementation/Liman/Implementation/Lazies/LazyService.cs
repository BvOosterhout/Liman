using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.Lazies
{
    [LimanService(LimanServiceLifetime.Transient)]
    internal class LazyService<T>(IServiceProvider serviceProvider) : Lazy<T>(serviceProvider.GetRequiredService<T>)
        where T : notnull
    {
    }
}
