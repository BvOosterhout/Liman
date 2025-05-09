using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman
{
    internal interface ILimanRunnableAsync
    {
        public Task Run();

        public void Stop();
    }
}
