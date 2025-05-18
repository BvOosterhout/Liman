namespace Liman.WikiExamples;

[LimanService]
internal class MyNotDisposableService
{
    private IMyDisposableService myService;

    public MyNotDisposableService(IMyDisposableService myService)
    {
        this.myService = myService;
    }
}
