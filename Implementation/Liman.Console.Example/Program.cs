// See https://aka.ms/new-console-template for more information
using Liman;
using System.Reflection;

var serviceCollection = LimanFactory.CreateServiceCollection();
serviceCollection.Add(Assembly.GetExecutingAssembly());
var application = LimanFactory.CreateApplication(serviceCollection);
application.Run();