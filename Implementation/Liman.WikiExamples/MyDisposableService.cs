namespace Liman.WikiExamples;

[LimanService]
public class MyDisposableService : IMyDisposableService, IDisposable
{
    public void Dispose()
    {
        // Code to clean up stuff
    }
}
