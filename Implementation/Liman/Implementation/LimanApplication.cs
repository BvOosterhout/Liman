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
        private readonly ILimanServiceProvider serviceProvider;
        private IRunnable? runnable;

        public LimanApplication(ILimanServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Run()
        {
            if (serviceProvider is LimanServiceProvider limanServiceProvider)
            {
                var applicationServices = limanServiceProvider.GetApplicationServices().ToList();
                var runnables = applicationServices.OfType<IRunnable>().ToList();

                if (runnables.Count > 1) throw new Exception("More than one runnable service found");

                runnable = runnables.FirstOrDefault();
                runnable?.Run();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Stop()
        {
            runnable?.Stop();
        }
    }
}
