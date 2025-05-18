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

        [Fact]
        public void ScopedService_WhenScopeIsDisposed_ServiceIsDisposed()
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyLifetimeLogger), LimanServiceLifetime.Scoped);
            limanServiceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = (IServiceProvider)classicServiceCollection.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var serviceInstance = scope.ServiceProvider.GetRequiredService<MyLifetimeLogger>();

            // Act
            scope.Dispose();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(x => x.Action == LifetimeLogAction.Disposed && x.Service == serviceInstance);
        }

        [Fact]
        public void ScopedService_WhenScopeIsDisposed_TransientChildServiceIsDisposed()
        {
            // Arrange
            limanServiceCollection.Add(typeof(ParentLifetimeLogger), LimanServiceLifetime.Scoped);
            limanServiceCollection.Add(typeof(ChildLifetimeLogger), LimanServiceLifetime.Transient);
            limanServiceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = (IServiceProvider)classicServiceCollection.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var serviceInstance = scope.ServiceProvider.GetRequiredService<ParentLifetimeLogger>();

            // Act
            scope.Dispose();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(x => x.Action == LifetimeLogAction.Disposed && x.Service == serviceInstance.Child);
        }

        [Fact]
        public void SingletonService_WhenServiceProviderIsDisposed_ServiceIsDisposed()
        {
            // Arrange
            limanServiceCollection.Add(typeof(MyLifetimeLogger), LimanServiceLifetime.Singleton);
            limanServiceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = classicServiceCollection.BuildServiceProvider();
            var serviceInstance = serviceProvider.GetRequiredService<MyLifetimeLogger>();
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();

            // Act
            serviceProvider.Dispose();

            // Assert
            lifetimeLog.Should().ContainSingle(x => x.Action == LifetimeLogAction.Disposed && x.Service == serviceInstance);
        }

        [Fact]
        public void SingletonService_WhenServiceProviderIsDisposed_TransientChildServiceIsDisposed()
        {
            // Arrange
            limanServiceCollection.Add(typeof(ParentLifetimeLogger), LimanServiceLifetime.Singleton);
            limanServiceCollection.Add(typeof(ChildLifetimeLogger), LimanServiceLifetime.Singleton);
            limanServiceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
            limanServiceCollection.ApplyTo(classicServiceCollection);
            var serviceProvider = classicServiceCollection.BuildServiceProvider();
            var serviceInstance = serviceProvider.GetRequiredService<ParentLifetimeLogger>();
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();

            // Act
            serviceProvider.Dispose();

            // Assert
            lifetimeLog.Should().ContainSingle(x => x.Action == LifetimeLogAction.Disposed && x.Service == serviceInstance.Child);
        }


        public interface IMyService
        {
        }

        public class MyServiceImplementation: IMyService
        {

        }

        public class ParentLifetimeLogger : ILimanInitializable, IDisposable
        {
            protected readonly LifetimeLog log;

            public ChildLifetimeLogger Child { get; }

            public ParentLifetimeLogger(LifetimeLog log, ChildLifetimeLogger child)
            {
                this.log = log;
                Child = child;
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

        public class ChildLifetimeLogger : ILimanInitializable, IDisposable
        {
            protected readonly LifetimeLog log;

            public ChildLifetimeLogger(LifetimeLog log)
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
