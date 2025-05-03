using System.Diagnostics.CodeAnalysis;

namespace Liman.Implementation.ServiceImplementations
{
    internal interface ILimanServiceImplementationRepository : ILimanServiceCollection
    {
        IEnumerable<LimanServiceImplementation> GetAll(Type serviceType);
        IEnumerable<LimanServiceImplementation> GetApplicationImplementations();
        bool TryGet(Type serviceType, [MaybeNullWhen(false)] out LimanServiceImplementation serviceImplementation);
    }
}
