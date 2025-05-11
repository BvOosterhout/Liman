# What is it?
Liman is a library for C# that helps with Service **Li**fetime **Man**agement. This encompasses the following functionalities:
- Service location/creation
- Dependency injection
- Service disposal

- And more...

# Main features
This chapter will give you a brief overview of the main features of Liman. For more detailed information, please check out [the Wiki](https://github.com/BvOosterhout/Liman/wiki/Home).

## Registration via Attribute
In Liman, the most common way to register your service implementation will be by adding an attribute to your class. This keeps information about the lifetime of your class inside the file that contains the class. Allowing you to adhere better to the single responsibility principle.
```csharp
[LimanService]
public class MyServiceImplementation : IMyService
{
}
```

## Constructor injection
Constructor injection is the main way to get access to services. This ensures you have immediate access to services that you depend on.

```csharp
[LimanService]
internal class MyServiceWithInjection
{
    private IMyService myService;

    public MyServiceWithInjection(IMyService myService)
    {
        this.myService = myService;
    }
}
```

## Lifetime management
Liman will automatically clean up your disposables when they are no longer used, or at least when your application is finished. Even if the service that uses it, is not disposable.

```csharp
[LimanService]
public class MyDisposableService : IMyDisposableService, IDisposable
{
    public void Dispose()
    {
        // Code to clean up stuff
    }
}
```

```csharp
[LimanService]
internal class MyNotDisposableService
{
    private IMyDisposableService myService;

    public MyNotDisposableService(IMyDisposableService myService)
    {
        this.myService = myService;
    }
}
```

_Program.cs_
```csharp
using Liman;
using System.Reflection;

var serviceCollection = LimanFactory.CreateServiceCollection();
serviceCollection.Add(Assembly.GetExecutingAssembly());
var application = LimanFactory.CreateApplication(serviceCollection);
application.Run();
```

It will also automatically load (and run) your application services.

```csharp
[LimanService(LimanServiceLifetime.Application)]
internal class MyApplicationService : ILimanRunnable
{
    private readonly IMyService service;
    private bool isRunning = true;

    public MyApplicationService(IMyService service)
    {
        this.service = service;
    }

    public void Run()
    {
        Console.WriteLine("Press 'Enter' to do something, or type 'quit' to exit.");

        while (isRunning)
        {
            var line = Console.ReadLine();

            if (line == "quit")
            {
                isRunning = false;
            }
            else
            {
                service.DoSomething();
            }
        }
    }
}
```

```csharp
[LimanService(LimanServiceLifetime.Application)]
internal class ApplicationLogger : ILimanInitializable, IDisposable
{
    public void Initialize()
    {
        Console.WriteLine("Application started");
    }
    
    public void Dispose()
    {
        Console.WriteLine("Application stopped");
    }   
}
```

And if you are creating services outside of constructor injection, make sure you let the service provider know when you don't use it anymore. This ensures that everything is disposed of properly.

```csharp
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
```

# Custom arguments
In some cases you may need dependency injection and arguments in the same constructor. You can use the NoInjectionAttribute for this.

Keep in mind that you have to put your custom parameters at the end of the constructor. And when creating the service, you need to enter the arguments in the same order.

```csharp
[LimanService(LimanServiceLifetime.Transient)]
internal class CustomParametersService(IMyService aService, [NoInjection] string name)
{
    public IMyService AService { get; } = aService;
    public string Name { get; } = name;
}
```

```csharp
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
```