

using Liman.Implementation.ServiceImplementations;

namespace Liman.Implementation.ServiceFactories
{
    internal interface IServiceFactoryProvider
    {
        IServiceFactory Get(Type serviceType);
        IServiceFactory[] GetUsedServices(LimanServiceImplementation parentImplementation);
        void FinishCreation(LimanServiceImplementation serviceImplementation, object result);
        void PrepareCreation(LimanServiceImplementation serviceImplementation);
        IEnumerable<IServiceFactory> GetApplicationServices();
    }
}