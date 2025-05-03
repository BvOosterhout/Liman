namespace Liman
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceImplementationAttribute : Attribute
    {
        public ServiceImplementationAttribute(ServiceImplementationLifetime lifetime = ServiceImplementationLifetime.Any, params Type[] serviceTypes)
        {
            Lifetime = lifetime;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public ServiceImplementationAttribute(params Type[] serviceTypes)
        {
            Lifetime = ServiceImplementationLifetime.Any;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public ServiceImplementationLifetime Lifetime { get; }
        public IReadOnlyList<Type> ServiceTypes { get; }
    }
}
