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
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Transient);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var service1 = serviceProvider.GetService<MyServiceImplementation>();
            var service2 = serviceProvider.GetService<MyServiceImplementation>();

            // Assert
            service1.Should().NotBeSameAs(service2);
        }

        [Theory()]
        [InlineData(LimanServiceLifetime.Singleton)]
        [InlineData(LimanServiceLifetime.Application)]
        public void NonTransient_ReusesInstances(LimanServiceLifetime lifetime)
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
        [InlineData(LimanServiceLifetime.Singleton)]
        [InlineData(LimanServiceLifetime.Application)]
        public void NonTransient_DifferentScopes_ReusesInstances(LimanServiceLifetime lifetime)
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
        [InlineData(LimanServiceLifetime.Scoped)]
        [InlineData(LimanServiceLifetime.Any)]
        public void Scoped_RequiresScope(LimanServiceLifetime lifetime)
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
        [InlineData(LimanServiceLifetime.Scoped)]
        [InlineData(LimanServiceLifetime.Any)]
        public void Scoped_ReusesInstanceWithinScope(LimanServiceLifetime lifetime)
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
        [InlineData(LimanServiceLifetime.Scoped)]
        [InlineData(LimanServiceLifetime.Any)]
        public void Scoped_CreatesNewInstanceForEachScope(LimanServiceLifetime lifetime)
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

        private Type PrepareScopedImplementation(LimanServiceLifetime lifetime)
        {
            Type serviceType;
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Scoped);

            if (lifetime == LimanServiceLifetime.Scoped)
            {
                serviceType = typeof(MyServiceImplementation);
            }
            else if (lifetime == LimanServiceLifetime.Any)
            {
                serviceCollection.Add(typeof(MyDependentServiceImplementation), LimanServiceLifetime.Any);
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

