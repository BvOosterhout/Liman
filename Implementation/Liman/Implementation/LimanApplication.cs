using Liman.Implementation.Lifetimes;
using Liman.Implementation.ServiceProviders;

namespace Liman.Implementation
{
    internal class LimanApplication : ILimanApplication
    {
        private readonly LimanServiceProvider serviceProvider;
        private ILimanRunnable? runnable;

        public LimanApplication(ILimanServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider as LimanServiceProvider
                ?? throw new InvalidOperationException();
        }

        public ILimanServiceProvider ServiceProvider { get => serviceProvider; }

        public void Run()
        {
            var applicationServices = serviceProvider.GetApplicationServices().ToList();

            // Start async runnables
            var asyncRunnables = applicationServices.OfType<ILimanRunnableAsync>().ToList();
            var runnableTasks = asyncRunnables.Select(r => r.Run()).ToArray();

            // Start main runnable
            var runnables = applicationServices.OfType<ILimanRunnable>().ToList();
            if (runnables.Count > 1) throw new LimanException("More than one IRunnable service found");

            runnable = runnables.FirstOrDefault();
            runnable?.Run();

            // clean up async runnables
            asyncRunnables.ForEach(r => r.Stop());
            Task.WaitAll(runnableTasks);

            // clean up services
            ServiceProvider.Dispose();
        }
    }
}
