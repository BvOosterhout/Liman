using Liman.Implementation.ServiceProviders;

namespace Liman.Implementation
{
    internal class LimanApplication(ILimanServiceProvider serviceProvider) : ILimanApplication
    {
        private readonly LimanServiceProvider serviceProvider = serviceProvider as LimanServiceProvider
                ?? throw new InvalidOperationException();
        private ILimanRunnable? runnable;

        public ILimanServiceProvider ServiceProvider { get => serviceProvider; }

        public void Run()
        {
            var applicationServices = serviceProvider.GetApplicationServices().ToList();

            // Start async runnables
            var asyncRunnables = applicationServices.OfType<ILimanRunnableAsync>().ToList();
            var runnableTasks = asyncRunnables.Select(r => r.Run()).ToArray();

            // Start main runnable
            var runnables = applicationServices.OfType<ILimanRunnable>().ToList();
            if (runnables.Count > 1) throw new LimanException($"More than one {nameof(ILimanRunnable)} service found");

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
