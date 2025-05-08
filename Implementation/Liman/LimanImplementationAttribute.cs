namespace Liman
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LimanImplementationAttribute : Attribute
    {
        public LimanImplementationAttribute(LimanImplementationLifetime lifetime = LimanImplementationLifetime.Any, params Type[] serviceTypes)
        {
            Lifetime = lifetime;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public LimanImplementationAttribute(params Type[] serviceTypes)
        {
            Lifetime = LimanImplementationLifetime.Any;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public LimanImplementationLifetime Lifetime { get; }
        public IReadOnlyList<Type> ServiceTypes { get; }
    }
}
