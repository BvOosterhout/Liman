using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
internal class CircularDependencyParent(CircularDependencyChild child)
{
    public CircularDependencyChild Child { get => Child; }
}
