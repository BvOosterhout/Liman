using System.Reflection;

namespace Liman
{
    public interface ILimanImplementation
    {
        Type Type { get; }
        LimanServiceLifetime Lifetime { get; }
        IReadOnlyList<Type> ServiceParameters { get; }
        IReadOnlyList<Type> CustomParameters { get; }
        Delegate? FactoryMethod { get; }
        ConstructorInfo? Constructor { get; }

        object CreateInstance(object?[] arguments);
    }
}