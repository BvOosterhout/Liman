namespace Liman
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LimanServiceAttribute : Attribute
    {
        public LimanServiceAttribute(LimanServiceLifetime lifetime = LimanServiceLifetime.Any, params Type[] serviceTypes)
        {
            Lifetime = lifetime;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public LimanServiceAttribute(params Type[] serviceTypes)
        {
            Lifetime = LimanServiceLifetime.Any;
            ServiceTypes = serviceTypes.AsReadOnly();
        }

        public LimanServiceLifetime Lifetime { get; }
        public IReadOnlyList<Type> ServiceTypes { get; }
    }
}
