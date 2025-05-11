using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
public class MyDisposableService : IMyDisposableService, IDisposable
{
    public void Dispose()
    {
        // Code to clean up stuff
    }
}
