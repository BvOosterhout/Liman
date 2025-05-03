namespace Liman
{
    public interface ILimanServiceProvider : IServiceProvider
    {
        object? GetService(Type serviceType, params object[] customArguments);
    }
}