namespace Liman.Tests.Helpers
{
    public class MyLifetimeLogger : ILimanInitializable, IDisposable
    {
        protected readonly LifetimeLog log;

        public MyLifetimeLogger(LifetimeLog log)
        {
            this.log = log;
            log.Log(LifetimeLogAction.Construct, this);
        }

        public void Initialize()
        {
            log.Log(LifetimeLogAction.Initialized, this);
        }

        public void Dispose()
        {
            log.Log(LifetimeLogAction.Disposed, this);
            GC.SuppressFinalize(this);
        }
    }
}
