using FluentAssertions;
using Liman.Tests.AssemblyInject;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
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
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(LimanServiceLifetime.Any);
        }

        [Fact]
        public void Add_MultipleServices()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Any);

            // Assert
            VerifyExistence<IMyService>(LimanServiceLifetime.Any);
            VerifyExistence<MyServiceImplementation>(LimanServiceLifetime.Any);
        }

        [Fact]
        public void Add_MultipleImplementations()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(AlternateServiceImplementation), LimanServiceLifetime.Any);

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
            serviceCollection.Add(typeof(List<>), LimanServiceLifetime.Any);

            // Assert
            VerifyExistence<List<string>>(LimanServiceLifetime.Any);
        }

        [Fact]
        public void Add_Twice()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Any);

            // Assert
            VerifyExistence<MyServiceImplementation>(LimanServiceLifetime.Any);
        }

        [Fact]
        public void Add_ViaAssembly()
        {
            // Arrange

            // Act
            serviceCollection.Add(Assembly.GetExecutingAssembly());

            // Assert
            VerifyExistence<AssemblyServiceImplementation>(LimanServiceLifetime.Any);
        }

        [Fact]
        public void Add_ViaDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(DependencyConfiguration));

            // Assert
            VerifyExistence<List<string>>(LimanServiceLifetime.Transient);
        }

        [Fact]
        public void Add_ViaClassicDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(ClassicDependencyConfiguration));

            // Assert
            VerifyExistence<Stack<string>>(LimanServiceLifetime.Transient);
        }

        [Fact]
        public void Add_WithFactoryMethod_ViaClassicDependencyConfiguration()
        {
            // Arrange

            // Act
            serviceCollection.Add(typeof(ClassicDependencyConfigurationWithFactory));

            // Assert
            VerifyExistence<IEnumerable>(LimanServiceLifetime.Transient, [typeof(IServiceProvider)]);
        }

        private void VerifyExistence<T>(LimanServiceLifetime lifetime, Type[]? serviceParameters = null)
        {
            VerifyExistence(typeof(T), lifetime, serviceParameters);
        }

        private void VerifyExistence(Type serviceType, LimanServiceLifetime lifetime, Type[]? serviceParameters = null)
        {
            serviceCollection.TryGetSingle(serviceType, out var implementation).Should().BeTrue();
            implementation.Should().NotBeNull();
            implementation.Lifetime.Should().Be(lifetime);
            implementation.ServiceParameters.Should().BeEquivalentTo(serviceParameters ?? []);
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
                services.Add(typeof(List<>), LimanServiceLifetime.Transient);
            }
        }

        public class ClassicDependencyConfiguration : ILimanClassicDependencyConfiguration
        {
            public void Configure(IServiceCollection services)
            {
                services.AddTransient(typeof(Stack<>), typeof(Stack<>));
            }
        }

        public class ClassicDependencyConfigurationWithFactory : ILimanClassicDependencyConfiguration
        {
            public void Configure(IServiceCollection services)
            {
                services.AddTransient(typeof(IEnumerable), classicServiceProvider => new Stack<string>());
            }
        }
    }
}
