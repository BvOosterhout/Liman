namespace Liman.WikiExamples;

[LimanService]
internal class MyServiceWithInjection
{
    private IMyService myService;

    public MyServiceWithInjection(IMyService myService)
    {
        this.myService = myService;
    }
}
