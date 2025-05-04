namespace Liman.ConsoleExample;

[ServiceImplementation]
public class MyServiceImplementation : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}
