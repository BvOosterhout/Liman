using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.WikiExamples;

[LimanService(LimanServiceLifetime.Application)]
internal class ServiceUser
{
    private ILimanServiceProvider serviceProvider;
    private IMyService? service;

    public ServiceUser(ILimanServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void CreateNew()
    {
        // remove old service
        if (service != null) serviceProvider.RemoveService(service);

        // create new service
        service = serviceProvider.GetRequiredService<IMyService>();
    }
}
