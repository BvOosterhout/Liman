namespace Liman
{
    public interface ILimanServiceProvider : IServiceProvider
    {
        object? GetService(Type serviceType, params object[] customArguments);
        void RemoveService(object service);

        void RegisterDependency(object dependency);
        void DeregisterDependency(object dependency);
        void RegisterDependency(object user, object dependency);
        void DeregisterDependency(object user, object dependency);
    }
}