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
            serviceCollection.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(ServiceImplementationLifetime.Any);
        }

        [Fact]
        public void Add_Twice()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Any);
            serviceCollection.Add(typeof(MyServiceImplementation), ServiceImplementationLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(ServiceImplementationLifetime.Any);
        }

        [Fact]
        public void Add_ViaAssembly()
        {
            // Arrange

            // Act
            serviceCollection.Add(Assembly.GetExecutingAssembly());

            // Assert
            VerifyExistence<AssemblyServiceImplementation>(ServiceImplementationLifetime.Any);
        }

        [Fact]
        public void Add_ViaDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(DependencyConfiguration));

            // Assert
            VerifyExistence<List<string>>(ServiceImplementationLifetime.Transient);
        }

        [Fact]
        public void Add_ViaClassicDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(ClassicDependencyConfiguration));

            // Assert
            VerifyExistence<Stack<string>>(ServiceImplementationLifetime.Transient);
        }

        private void VerifyExistence<T>(ServiceImplementationLifetime lifetime)
        {
            serviceCollection.TryGetSingle(typeof(T), out var implementation).Should().BeTrue();
            implementation.Should().NotBeNull();
            implementation.Type.Should().Be(typeof(T));
            implementation.Lifetime.Should().Be(lifetime);
            implementation.ServiceParameters.Should().BeEmpty();
            implementation.CustomParameters.Should().BeEmpty();
        }

        public class MyServiceImplementation
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
                services.AddTransient(typeof(Stack<>), typeof(Stack<>));
            }
        }
    }
}
