using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
internal class MyNotDisposableService
{
    private IMyDisposableService myService;

    public MyNotDisposableService(IMyDisposableService myService)
    {
        this.myService = myService;
    }
}
