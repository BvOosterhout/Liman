
namespace Liman.Implementation.Classics
{
    internal interface IClassicServiceProviderFactory
    {
        ILimanServiceProvider Get(IServiceProvider classicServiceProvider);
    }
}