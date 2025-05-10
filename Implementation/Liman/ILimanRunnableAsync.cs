namespace Liman
{
    public interface ILimanRunnableAsync
    {
        public Task Run();

        public void Stop();
    }
}
