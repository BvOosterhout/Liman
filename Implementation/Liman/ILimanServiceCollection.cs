using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Liman
{
    public interface ILimanServiceCollection
    {
        void Add(Type implementationType);
        void Add(Type implementationType, ServiceImplementationLifetime lifetime, Delegate? constructor = null);
        void Add(Type implementationType, ServiceImplementationLifetime lifetime, IEnumerable<Type> serviceTypes, Delegate? constructor = null);
        void Add(Assembly assembly, params Type[] exceptions);
        void ApplyTo(IServiceCollection classicServiceCollection);
        void Validate(Type serviceType);
        void ValidateAll();
    }
}
