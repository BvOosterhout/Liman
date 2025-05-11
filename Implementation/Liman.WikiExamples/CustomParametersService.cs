using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService(LimanServiceLifetime.Transient)]
internal class CustomParametersService(IMyService aService, [NoInjection] string name)
{
    public IMyService AService { get; } = aService;
    public string Name { get; } = name;
}
