using Liman.Implementation.ServiceFactories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation.Classics
{
    internal class ClassicServiceFactory: IServiceFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Type serviceType;

        public ClassicServiceFactory(IServiceProvider serviceProvider, Type serviceType)
        {
            this.serviceProvider = serviceProvider;
            this.serviceType = serviceType;
        }

        public LimanServiceLifetime Lifetime { get; } = LimanServiceLifetime.Any;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            if (scope != null) return scope.ServiceProvider.GetService(serviceType);
            else return serviceProvider.GetService(serviceType);
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
