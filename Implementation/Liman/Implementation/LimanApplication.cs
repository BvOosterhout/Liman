using Liman.Implementation.ServiceProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation
{
    internal class LimanApplication : ILimanApplication
    {
        private readonly LimanServiceProvider serviceProvider;
        private IRunnable? runnable;

        public LimanApplication(ILimanServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider as LimanServiceProvider
                ?? throw new InvalidOperationException();
        }

        public void Run()
        {
            var applicationServices = serviceProvider.GetApplicationServices().ToList();
            var runnables = applicationServices.OfType<IRunnable>().ToList();

            if (runnables.Count > 1) throw new LimanException("More than one IRunnable service found");

            runnable = runnables.FirstOrDefault();
            runnable?.Run();
        }

        public void Stop()
        {
            runnable?.Stop();
        }
    }
}
