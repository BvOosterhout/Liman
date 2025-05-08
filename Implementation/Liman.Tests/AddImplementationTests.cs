using FluentAssertions;
using Liman.Tests.AssemblyInject;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Liman.Tests
{
    /// <summary>
    /// Tests the different ways to add service implementations
    /// </summary>
    public class AddImplementationTests
    {
        private readonly ILimanServiceCollection serviceCollection;

        public AddImplementationTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
        }

        [Fact]
        public void NoAdd()
        {
            // Arrange

            // Act

            // Assert
            serviceCollection.TryGetSingle(typeof(MyServiceImplementation), out var implementation).Should().BeFalse();
        }

        [Fact]
        public void Add_Directly()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(LimanImplementationLifetime.Any);
        }

        [Fact]
        public void Add_MultipleServices()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Any);

            // Assert
            VerifyExistence<IMyService>(LimanImplementationLifetime.Any);
            VerifyExistence<MyServiceImplementation>(LimanImplementationLifetime.Any);
        }

        [Fact]
        public void Add_MultipleImplementations()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Any);
            serviceCollection.Add(typeof(AlternateServiceImplementation), LimanImplementationLifetime.Any);

            // Assert
            var implementations = serviceCollection.GetAll(typeof(IMyService));
            implementations.Should().ContainSingle(x => x.Type == typeof(MyServiceImplementation));
            implementations.Should().ContainSingle(x => x.Type == typeof(AlternateServiceImplementation));
        }

        [Fact]
        public void Add_Generic()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(List<>), LimanImplementationLifetime.Any);

            // Assert
            VerifyExistence<List<string>>(LimanImplementationLifetime.Any);
        }

        [Fact]
        public void Add_Twice()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Any);
            serviceCollection.Add(typeof(MyServiceImplementation), LimanImplementationLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(LimanImplementationLifetime.Any);
        }

        [Fact]
        public void Add_ViaAssembly()
        {
            // Arrange

            // Act
            serviceCollection.Add(Assembly.GetExecutingAssembly());

            // Assert
            VerifyExistence<AssemblyServiceImplementation>(LimanImplementationLifetime.Any);
        }

        [Fact]
        public void Add_ViaDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(DependencyConfiguration));

            // Assert
            VerifyExistence<List<string>>(LimanImplementationLifetime.Transient);
        }

        [Fact]
        public void Add_ViaClassicDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(ClassicDependencyConfiguration));

            // Assert
            VerifyExistence<Stack<string>>(LimanImplementationLifetime.Transient);
        }

        private void VerifyExistence<T>(LimanImplementationLifetime lifetime)
        {
            VerifyExistence(typeof(T), lifetime);
        }

        private void VerifyExistence(Type serviceType, LimanImplementationLifetime lifetime)
        {
            serviceCollection.TryGetSingle(serviceType, out var implementation).Should().BeTrue();
            implementation.Should().NotBeNull();
            implementation.Lifetime.Should().Be(lifetime);
            implementation.ServiceParameters.Should().BeEmpty();
            implementation.CustomParameters.Should().BeEmpty();
        }

        public interface IMyService
        {
        }

        public class MyServiceImplementation : IMyService
        {
        }

        public class AlternateServiceImplementation : IMyService
        {
        }

        public class DependencyConfiguration : ILimanDependencyConfiguration
        {
            public void Configure(ILimanServiceCollection services)
            {
                services.Add(typeof(List<>), LimanImplementationLifetime.Transient);
            }
        }

        public class ClassicDependencyConfiguration : ILimanClassicDependencyConfiguration
        {
            public void Configure(IServiceCollection services)
            {
                services.AddTransient(typeof(Stack<>), typeof(Stack<>));
            }
        }
    }
}
