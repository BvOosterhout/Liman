namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyParent(CircularDependencyChild child)
{
    public CircularDependencyChild Child { get => child; }
}
