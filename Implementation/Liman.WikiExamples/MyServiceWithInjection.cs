using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
internal class MyServiceWithInjection
{
    private IMyService myService;

    public MyServiceWithInjection(IMyService myService)
    {
        this.myService = myService;
    }
}
