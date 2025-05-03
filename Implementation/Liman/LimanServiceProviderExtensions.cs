namespace Liman
{
    public static class LimanServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this ILimanServiceProvider serviceProvider, params object[] customArguments)
        {
            var service = (T?)serviceProvider.GetService(typeof(T), customArguments) ?? throw new NullReferenceException();
            return service;
        }
    }
}
