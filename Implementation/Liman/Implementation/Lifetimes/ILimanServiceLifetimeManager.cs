namespace Liman.Implementation.Lifetimes
{
    public interface ILimanServiceLifetimeManager
    {
        bool AddTransientDependency(object user, object transient);
        void DeleteTransientDependency(object user, object transient);
        void DeleteAllServices();
        void Delete(object implementation);
        void AddSingleton(object singleton);
    }
}