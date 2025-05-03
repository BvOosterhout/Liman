namespace Liman
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class NoInjectionAttribute : Attribute
    {
        public NoInjectionAttribute() { }
    }
}
