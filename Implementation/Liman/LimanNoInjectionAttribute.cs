namespace Liman
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class LimanNoInjectionAttribute : Attribute
    {
        public LimanNoInjectionAttribute() { }
    }
}
