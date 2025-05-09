namespace Liman
{
    public interface ILimanServiceImplementation
    {
        Type Type { get; }
        LimanServiceLifetime Lifetime { get; }
        IReadOnlyList<Type> ServiceParameters { get; }
        IReadOnlyList<Type> CustomParameters { get; }

        object CreateInstance(object?[] arguments);
    }
}