using Liman.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Liman
{
    public static class LimanServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this ILimanServiceProvider serviceProvider, params object[] customArguments)
        {
            var service = (T?)serviceProvider.GetService(typeof(T), customArguments)
                ?? throw new LimanException($"Could not find required service '{typeof(T).GetReadableName()}'");
            return service;
        }

        public static object GetRequiredService(this ILimanServiceProvider serviceProvider, ILimanImplementation implementationType,  params object[] customArguments)
        {
            var service = serviceProvider.GetService(implementationType, customArguments)
                ?? throw new LimanException($"Could not find required service '{implementationType}'");
            return service;
        }
    }
}
