using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            serviceCollection.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Transient);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var service1 = serviceProvider.GetService<MyServiceImplementation>();
            var service2 = serviceProvider.GetService<MyServiceImplementation>();

            // Assert
            service1.Should().NotBeSameAs(service2);
        }

        [Theory()]
        [InlineData(ServiceImplementationLifetime.Singleton)]
        [InlineData(ServiceImplementationLifetime.Application)]
        public void NonTransient_ReusesInstances(ServiceImplementationLifetime lifetime)
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
        [InlineData(ServiceImplementationLifetime.Singleton)]
        [InlineData(ServiceImplementationLifetime.Application)]
        public void NonTransient_DifferentScopes_ReusesInstances(ServiceImplementationLifetime lifetime)
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
        [InlineData(ServiceImplementationLifetime.Scoped)]
        [InlineData(ServiceImplementationLifetime.Any)]
        public void Scoped_RequiresScope(ServiceImplementationLifetime lifetime)
        {
            // Arrange
            Type serviceType = PrepareScopedImplementation(lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var action =() => serviceProvider.GetService(serviceType);

            // Assert
            action.Should().Throw<LimanException>();
        }

        [Theory]
        [InlineData(ServiceImplementationLifetime.Scoped)]
        [InlineData(ServiceImplementationLifetime.Any)]
        public void Scoped_ReusesInstanceWithinScope(ServiceImplementationLifetime lifetime)
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
        [InlineData(ServiceImplementationLifetime.Scoped)]
        [InlineData(ServiceImplementationLifetime.Any)]
        public void Scoped_CreatesNewInstanceForEachScope(ServiceImplementationLifetime lifetime)
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

        private Type PrepareScopedImplementation(ServiceImplementationLifetime lifetime)
        {
            Type serviceType;
            serviceCollection.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Scoped);

            if (lifetime == ServiceImplementationLifetime.Scoped)
            {
                serviceType = typeof(MyServiceImplementation);
            }
            else if (lifetime == ServiceImplementationLifetime.Any)
            {
                serviceCollection.Add(typeof(MyDependentServiceImplementation), ServiceImplementationLifetime.Any);
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

