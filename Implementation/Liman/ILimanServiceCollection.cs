using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Liman
{
    public interface ILimanServiceCollection
    {
        // Add methods
        void Add(Type implementationType);
        void Add(Type implementationType, LimanImplementationLifetime lifetime, Delegate? constructor = null);
        void Add(Type implementationType, LimanImplementationLifetime lifetime, IEnumerable<Type> serviceTypes, Delegate? constructor = null);
        void Add(Assembly assembly, params Type[] exceptions);

        // Get methods
        bool TryGetSingle(Type serviceType, [MaybeNullWhen(false)] out ILimanServiceImplementation serviceImplementation);
        IEnumerable<ILimanServiceImplementation> GetAll(Type serviceType);
        IEnumerable<ILimanServiceImplementation> GetApplicationImplementations();
        LimanImplementationLifetime GetEffectiveLifetime(ILimanServiceImplementation implementation);

        // Validation methods
        void Validate(ILimanServiceImplementation implementation);
        void Validate(Type serviceType);
        void ValidateAll();
    }
}
