namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyChild2(Lazy<CircularDependencyParent> parent) : ILimanInitializable
{
    public void Initialize()
    {
        var myParentValue = parent.Value;
    }
}
