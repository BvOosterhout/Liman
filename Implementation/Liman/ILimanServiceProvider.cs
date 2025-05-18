namespace Liman
{
    public interface ILimanServiceProvider : IServiceProvider, IDisposable
    {
        object? GetService(Type serviceType, params object[] customArguments);
        void RemoveService(object service);

        void RegisterDependency(object dependency);
        void DeregisterDependency(object dependency);
        void RegisterDependency(object user, object dependency);
        void DeregisterDependency(object user, object dependency);
        object? GetService(ILimanImplementation implementationType, params object[] customArguments);
    }
}