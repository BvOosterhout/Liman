namespace Liman.ConsoleExample;

[LimanService]
public class MyServiceImplementation : IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("Doing something...");
    }
}
