using FluentAssertions;

namespace Liman.Tests
{
    public class CustomArgumentTests
    {
        ILimanServiceCollection serviceCollection;

        public CustomArgumentTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
            serviceCollection.Add(typeof(MyServiceWithCustomParameter), LimanImplementationLifetime.Transient);
        }

        [Fact]
        public void CustomParameter_WhenPassed_Succeeds()
        {
            // Arrange
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var service = serviceProvider.GetRequiredService<MyServiceWithCustomParameter>("TestValue");

            // Assert
            service.CustomArgument.Should().Be("TestValue");
        }

        [Fact]
        public void CustomParameter_WhenNotPassed_ThrowsException()
        {
            // Arrange
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var action = () => serviceProvider.GetRequiredService<MyServiceWithCustomParameter>();

            // Assert
            action.Should().Throw<LimanException>();
        }

        [Fact]
        public void CustomParameter_WhenTooManyPassed_ThrowsException()
        {
            // Arrange
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var action = () => serviceProvider.GetRequiredService<MyServiceWithCustomParameter>("Argument1", "Argument2");

            // Assert
            action.Should().Throw<LimanException>();
        }

        public class MyServiceWithCustomParameter
        {
            public MyServiceWithCustomParameter(IServiceProvider dependency, [LimanNoInjection] string customArgument)
            {
                Dependency = dependency;
                CustomArgument = customArgument;
            }

            public IServiceProvider Dependency { get; }
            public string CustomArgument { get; }
        }
    }
}
