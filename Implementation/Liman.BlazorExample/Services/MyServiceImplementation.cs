using Liman.BlazorExample.Services;

namespace Liman.ConsoleExample;

[LimanService]
public class MyServiceImplementation : IMyService
{
    public string Message()
    {
        return "Hello from MyServiceImplementation!";
    }
}
