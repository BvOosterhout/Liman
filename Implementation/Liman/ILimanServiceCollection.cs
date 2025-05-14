using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Liman
{
    public interface ILimanServiceCollection
    {
        // Add methods
        void Add(Type implementationType);
        void Add(Type implementationType, LimanServiceLifetime lifetime, Delegate? constructor = null);
        void Add(Type implementationType, LimanServiceLifetime lifetime, IEnumerable<Type> serviceTypes, Delegate? constructor = null);
        void Add(Assembly assembly, params Type[] exceptions);

        // Get methods
        bool TryGetSingle(Type serviceType, [MaybeNullWhen(false)] out ILimanImplementation serviceImplementation);
        IEnumerable<ILimanImplementation> GetAll(Type serviceType);
        IEnumerable<ILimanImplementation> GetApplicationImplementations();
        LimanServiceLifetime GetEffectiveLifetime(ILimanImplementation implementation);
        IEnumerable<KeyValuePair<Type, ILimanImplementation>> GetAllServiceImplementations();

        // Validation methods
        void Validate(ILimanImplementation implementation);
        void Validate(Type serviceType);
        void ValidateAll();
    }
}
