using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Tests
{
    public class LifetimeTests
    {
        private ILimanServiceCollection serviceCollection;

        public LifetimeTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
        }

        [Fact]
        public void Transient_CreatesNewInstanceForEveryCall()
        {
            // Arrange
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Transient);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var service1 = serviceProvider.GetService<MyServiceImplementation>();
            var service2 = serviceProvider.GetService<MyServiceImplementation>();

            // Assert
            service1.Should().NotBeSameAs(service2);
        }

        [Theory()]
        [InlineData(LimanImplementationLifetime.Singleton)]
        [InlineData(LimanImplementationLifetime.Application)]
        public void NonTransient_ReusesInstances(LimanImplementationLifetime lifetime)
        {
            // Arrange
            serviceCollection.Add(typeof(MyServiceImplementation), lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var service1 = serviceProvider.GetService<MyServiceImplementation>();
            var service2 = serviceProvider.GetService<MyServiceImplementation>();

            // Assert
            service1.Should().BeSameAs(service2);
        }

        [Theory()]
        [InlineData(LimanImplementationLifetime.Singleton)]
        [InlineData(LimanImplementationLifetime.Application)]
        public void NonTransient_DifferentScopes_ReusesInstances(LimanImplementationLifetime lifetime)
        {
            // Arrange
            var serviceType = typeof(MyServiceImplementation);
            serviceCollection.Add(serviceType, lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);
            var scopedServiceProvider1 = serviceProvider.CreateScope().ServiceProvider;
            var scopedServiceProvider2 = serviceProvider.CreateScope().ServiceProvider;

            // Act
            var service1 = scopedServiceProvider1.GetService(serviceType);
            var service2 = scopedServiceProvider2.GetService(serviceType);

            // Assert
            service1.Should().BeSameAs(service2);
        }

        [Theory]
        [InlineData(LimanImplementationLifetime.Scoped)]
        [InlineData(LimanImplementationLifetime.Any)]
        public void Scoped_RequiresScope(LimanImplementationLifetime lifetime)
        {
            // Arrange
            Type serviceType = PrepareScopedImplementation(lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var action = () => serviceProvider.GetService(serviceType);

            // Assert
            action.Should().Throw<LimanException>();
        }

        [Theory]
        [InlineData(LimanImplementationLifetime.Scoped)]
        [InlineData(LimanImplementationLifetime.Any)]
        public void Scoped_ReusesInstanceWithinScope(LimanImplementationLifetime lifetime)
        {
            // Arrange
            Type serviceType = PrepareScopedImplementation(lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);
            var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            // Act
            var service1 = scopedServiceProvider.GetService(serviceType);
            var service2 = scopedServiceProvider.GetService(serviceType);

            // Assert
            service1.Should().BeSameAs(service2);
        }

        [Theory]
        [InlineData(LimanImplementationLifetime.Scoped)]
        [InlineData(LimanImplementationLifetime.Any)]
        public void Scoped_CreatesNewInstanceForEachScope(LimanImplementationLifetime lifetime)
        {
            // Arrange
            Type serviceType = PrepareScopedImplementation(lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);
            var scopedServiceProvider1 = serviceProvider.CreateScope().ServiceProvider;
            var scopedServiceProvider2 = serviceProvider.CreateScope().ServiceProvider;

            // Act
            var service1 = scopedServiceProvider1.GetService(serviceType);
            var service2 = scopedServiceProvider2.GetService(serviceType);

            // Assert
            service1.Should().NotBeSameAs(service2);
        }

        private Type PrepareScopedImplementation(LimanImplementationLifetime lifetime)
        {
            Type serviceType;
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Scoped);

            if (lifetime == LimanImplementationLifetime.Scoped)
            {
                serviceType = typeof(MyServiceImplementation);
            }
            else if (lifetime == LimanImplementationLifetime.Any)
            {
                serviceCollection.Add(typeof(MyDependentServiceImplementation), LimanImplementationLifetime.Any);
                serviceType = typeof(MyDependentServiceImplementation);
            }
            else
            {
                throw new NotSupportedException();
            }

            return serviceType;
        }

        public class MyServiceImplementation
        {
        }

        public class MyDependentServiceImplementation
        {
            public MyDependentServiceImplementation(MyServiceImplementation service)
            {
                Service = service;
            }
            public MyServiceImplementation Service { get; }
        }
    }
}

