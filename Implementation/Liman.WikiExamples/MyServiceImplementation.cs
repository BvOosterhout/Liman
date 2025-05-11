using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService]
public class MyServiceImplementation : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}
