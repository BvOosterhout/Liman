using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyChild(Lazy<CircularDependencyParent> parent)
{
    public CircularDependencyParent Parent { get => parent.Value; }
}
