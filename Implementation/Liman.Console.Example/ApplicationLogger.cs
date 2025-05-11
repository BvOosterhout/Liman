using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.ConsoleExample;

[LimanService(LimanServiceLifetime.Application)]
internal class ApplicationLogger : ILimanInitializable, IDisposable
{
    public void Initialize()
    {
        Console.WriteLine("Application started");
    }

    public void Dispose()
    {
        Console.WriteLine("Application stopped");
    }
}
