

using Liman.Implementation.ServiceImplementations;

namespace Liman.Implementation.ServiceFactories
{
    internal interface IServiceFactoryProvider
    {
        IServiceFactory Get(Type serviceType);
        IServiceFactory[] GetUsedServices(ILimanServiceImplementation parentImplementation);
        void FinishCreation(ILimanServiceImplementation serviceImplementation, object result);
        void PrepareCreation(ILimanServiceImplementation serviceImplementation);
        IEnumerable<IServiceFactory> GetApplicationServices();
    }
}