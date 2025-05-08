namespace Liman
{
    public interface ILimanApplication
    {
        public ILimanServiceProvider ServiceProvider { get; }

        public void Run();

        public void Stop();
    }
}
