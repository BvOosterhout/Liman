using Microsoft.Extensions.DependencyInjection;

namespace Liman
{
    public interface ILimanClassicDependencyConfiguration
    {
        public void Configure(IServiceCollection serviceCollection);
    }
}
