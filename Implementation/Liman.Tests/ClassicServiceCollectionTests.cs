using FluentAssertions;
using Liman.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Liman.Tests
{
    public class ClassicServiceCollectionTests
    {
        private ILimanServiceCollection limanServiceCollection;
        private IServiceCollection classicServiceCollection;

        public ClassicServiceCollectionTests()
        {
            limanServiceCollection = LimanFactory.CreateServiceCollection();
            classicServiceCollection = new ServiceCollection();
        }

        [Theory()]
        [InlineData(LimanServiceLifetime.Singleton)]
        [InlineData(LimanServiceLifetime.Application)]
        public void Singleton_RetrievesSameInstance(LimanServiceLifetime lifetime)
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyServiceImplementation), lifetime);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = classicServiceCollection.BuildServiceProvider();

            // Act
            var serviceInstance = serviceProvider.GetRequiredService<IMyService>();
            var implementationInstance = serviceProvider.GetRequiredService<MyServiceImplementation>();

            // Assert
            serviceInstance.Should().BeSameAs(implementationInstance);
        }

        [Fact]
        public void Initialize_IsCalled()
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyLifetimeLogger), LimanServiceLifetime.Transient);
            limanServiceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = classicServiceCollection.BuildServiceProvider();

            // Act
            var serviceInstance = serviceProvider.GetRequiredService<MyLifetimeLogger>();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().Contain(x => x.Action == LifetimeLogAction.Initialized && x.Service == serviceInstance);
        }

        [Fact]
        public void ScopedService_SameScope_RetrievesSameInstance()
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Scoped);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = (IServiceProvider)classicServiceCollection.BuildServiceProvider();
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;

            // Act
            var serviceInstance1 = serviceProvider.GetRequiredService<IMyService>();
            var serviceInstance2 = serviceProvider.GetRequiredService<IMyService>();

            // Assert
            serviceInstance1.Should().BeSameAs(serviceInstance2);
        }

        [Fact]
        public void ScopedService_DifferentScope_RetrievesDifferentInstance()
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyServiceImplementation), LimanServiceLifetime.Scoped);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = (IServiceProvider)classicServiceCollection.BuildServiceProvider();
            var scope1 = serviceProvider.CreateScope().ServiceProvider;
            var scope2 = serviceProvider.CreateScope().ServiceProvider;

            // Act
            var serviceInstance1 = scope1.GetRequiredService<IMyService>();
            var serviceInstance2 = scope2.GetRequiredService<IMyService>();

            // Assert
            serviceInstance1.Should().NotBeSameAs(serviceInstance2);
        }

        public interface IMyService
        {
        }

        public class MyServiceImplementation: IMyService
        {

        }

        public class MyLifetimeLogger : ILimanInitializable, IDisposable
        {
            protected readonly LifetimeLog log;

            public MyLifetimeLogger(LifetimeLog log)
            {
                this.log = log;
                log.Log(LifetimeLogAction.Construct, this);
            }

            public void Initialize()
            {
                log.Log(LifetimeLogAction.Initialized, this);
            }

            public void Dispose()
            {
                log.Log(LifetimeLogAction.Disposed, this);
                GC.SuppressFinalize(this);
            }
        }
    }
}
