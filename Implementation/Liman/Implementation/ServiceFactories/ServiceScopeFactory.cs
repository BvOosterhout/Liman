using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation.ServiceFactories
{
    internal class ServiceScopeFactory : IServiceFactory
    {
        public ServiceImplementationLifetime Lifetime => ServiceImplementationLifetime.Transient;

        public object? Get(IServiceScope? scope, object?[] customArguments)
        {
            return scope;
        }

        public void RegisterUser(object user, object dependency)
        {
        }
    }
}
