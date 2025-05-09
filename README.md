# What is it?
Liman is a library for C# that helps with Service **Li**fetime **Man**agement. This encompasses the following functionalities:
- Service location/creation
- Dependency injection
- Service disposal

- And some other related functionalities

# Main features

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
public class MyOtherServiceImplementation
{
    private IMyService myService;

    public MyOtherServiceImplementation(IMyService myService)
    {
        this.myService = myService
    }
}
```

## Lifetime management
Liman will automatically clean up your disposables when they are no longer used, or at least when your application is finished. Even if the service that uses it is not disposable.

```csharp
[LimanService]
public class MyDisposableService : IMyService, IDisposable
{
    public void Dispose()
    {
        // Code to clean up stuff
    }
}
```

```csharp
[LimanService]
public class MyNotDisposableService
{
    private IMyService myService;

    public MyNotDisposableService(IMyService myService)
    {
        this.myService = myService
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
internal class MainApplicationService : ILimanRunnable
{
    private bool isRunning = true;

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
                Console.WriteLine("Nice day for fishing, ain't it!?");
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
        // remove old service if needed
        if (service != null) serviceProvider.RemoveService(service);
        
        // create new service
        service = serviceProvider.GetRequiredService<IMyService>();
    }   
}
```
