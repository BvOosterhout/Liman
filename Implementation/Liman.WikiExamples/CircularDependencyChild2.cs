using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyChild2(Lazy<CircularDependencyParent> parent) : ILimanInitializable
{
    public void Initialize()
    {
        var myParentValue = parent.Value;
    }
}
