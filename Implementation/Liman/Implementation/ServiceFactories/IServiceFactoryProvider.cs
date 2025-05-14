namespace Liman.Implementation.ServiceFactories
{
    internal interface IServiceFactoryProvider
    {
        IServiceFactory Get(Type serviceType);
        IServiceFactory[] GetUsedServices(ILimanImplementation parentImplementation);
        void FinishCreation(ILimanImplementation serviceImplementation, object result);
        void PrepareCreation(ILimanImplementation serviceImplementation);
        IEnumerable<IServiceFactory> GetApplicationServices();
    }
}