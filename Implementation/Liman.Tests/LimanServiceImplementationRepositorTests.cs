namespace Liman.Tests
{
    /*public class LimanServiceImplementationRepositoryTests
    {
        private readonly LimanServiceImplementationRepository repository;

        public LimanServiceImplementationRepositoryTests()
        {
            repository = new LimanServiceImplementationRepository();
        }

        [Fact]
        public void GetApplicationImplementations_ReturnsApplicationLifetimeServices()
        {
            // Arrange
            var implementationType = typeof(MyServiceImplementation);
            repository.Add(implementationType, ServiceImplementationLifetime.Application);
            repository.Add(typeof(MyAlternateServiceImplementation), ServiceImplementationLifetime.Singleton);

            // Act
            var implementations = repository.GetApplicationImplementations().ToList();

            // Assert
            implementations.Should().HaveCount(1);
            implementations[0].Type.Should().Be(implementationType);
        }

        [Fact]
        public void ApplyTo_AddsRegisteredServicesToServiceCollection()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Add some implementations to the repository
            repository.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Singleton);
            repository.Add(typeof(MyServiceImplementationWithAttribute));
            repository.Add(typeof(IFactoryService), ServiceImplementationLifetime.Singleton, CreateFactoryService);

            // Act
            repository.ApplyTo(serviceCollection);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Verify that the services are registered with the correct lifetimes
            var services = serviceProvider.GetService<IEnumerable<ITestService>>();
            services.Should().ContainSingle(x => x.GetType() == typeof(MyServiceImplementation));
            services.Should().ContainSingle(x => x.GetType() == typeof(MyServiceImplementationWithAttribute));

            // verify that factory service is registered
            var factoryService = serviceProvider.GetService<IFactoryService>();
            factoryService.Should().BeOfType<FactoryImplementation>();
        }

        private object CreateFactoryService(IServiceProvider serviceProvider, IEnumerable<ITestService> testServices)
        {
            if (!testServices.Any()) throw new ArgumentException();

            return new FactoryImplementation(serviceProvider);
        }

        private interface ITestService
        {
        }

        private interface IFactoryService
        {
        }

        public class MyServiceImplementation : ITestService
        {
        }

        public class MyAlternateServiceImplementation : ITestService
        {
        }

        [ServiceImplementation(ServiceImplementationLifetime.Scoped)]
        public class MyServiceImplementationWithAttribute : ITestService
        {
        }

        public class DependencyConfiguration : ILimanDependencyConfiguration
        {
            public void Configure(ILimanServiceCollection services)
            {
                services.Add(typeof(List<>), ServiceImplementationLifetime.Transient);
            }
        }

        public class ClassicDependencyConfiguration : ILimanClassicDependencyConfiguration
        {
            public void Configure(IServiceCollection services)
            {
                services.AddTransient(typeof(IList<>), typeof(List<>));
            }
        }

        public class FactoryImplementation : IFactoryService
        {
            public FactoryImplementation(IServiceProvider serviceProvider)
            {
            }
        }
    }*/
}