using Liman.Implementation;

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
    }
}
