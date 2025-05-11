namespace Liman.WikiExamples;

[LimanService]
internal class CustomParametersFactory(ILimanServiceProvider serviceProvider)
{
    public CustomParametersService Create(string name)
    {
        return serviceProvider.GetRequiredService<CustomParametersService>(name);
    }

    public void Delete(CustomParametersService serviceToDelete)
    {
        serviceProvider.RemoveService(serviceToDelete);
    }
}
