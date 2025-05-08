namespace Liman.ConsoleExample;

[LimanImplementation]
public class MyServiceImplementation : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}
