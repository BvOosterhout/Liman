namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyChild(Lazy<CircularDependencyParent> parent)
{
    public CircularDependencyParent Parent { get => parent.Value; }
}
