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
            var runnables = applicationServices.OfType<ILimanRunnable>().ToList();

            if (runnables.Count > 1) throw new LimanException("More than one IRunnable service found");

            runnable = runnables.FirstOrDefault();
            runnable?.Run();

            var lifetimeManager = serviceProvider.GetRequiredService<ILimanServiceLifetimeManager>();
            lifetimeManager.DeleteAllServices();
        }

        public void Stop()
        {
            runnable?.Stop();
        }
    }
}
