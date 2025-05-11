using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Liman.Tests
{
    public class LifetimeManagementTests
    {
        private readonly ILimanServiceCollection serviceCollection;

        public LifetimeManagementTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
            serviceCollection.Add(typeof(LifetimeLog), LimanServiceLifetime.Singleton);
        }

        [Fact]
        public void Child_Initialize_CalledAfterParentConstruction()
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(MyParentService), LimanServiceLifetime.Any);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var parentService = serviceProvider.GetRequiredService<MyParentService>();
            var childService = parentService.Child;

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.IndexOf(LifetimeLogAction.Construct, parentService).Should()
                .BeLessThan(lifetimeLog.IndexOf(LifetimeLogAction.Initialized, childService));
        }

        [Fact]
        public void Application_ConstructsApplicationService()
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), LimanServiceLifetime.Application);
            var application = LimanFactory.CreateApplication(serviceCollection);
            var serviceProvider = application.ServiceProvider;

            // Act
            application.Run();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Construct && item.Service.GetType() == typeof(MyChildService));
        }

        [Fact]
        public void Application_RunsRunnableApplicationService()
        {
            // Arrange
            serviceCollection.Add(typeof(MyRunnableService), LimanServiceLifetime.Application);
            var application = LimanFactory.CreateApplication(serviceCollection);
            var serviceProvider = application.ServiceProvider;

            // Act
            application.Run();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Run && item.Service.GetType() == typeof(MyRunnableService));
        }

        [Fact]
        public void Application_RunsRunnableAsyncApplicationServices()
        {
            // Arrange
            serviceCollection.Add(typeof(MyAsyncRunnable1), LimanServiceLifetime.Application);
            serviceCollection.Add(typeof(MyAsyncRunnable2), LimanServiceLifetime.Application);
            var application = LimanFactory.CreateApplication(serviceCollection);
            var serviceProvider = application.ServiceProvider;
            var runnable1 = serviceProvider.GetRequiredService<MyAsyncRunnable1>();
            var runnable2 = serviceProvider.GetRequiredService<MyAsyncRunnable2>();

            // Act
            application.Run();

            // Assert
            runnable1.IsRunning.Should().BeFalse();
            runnable1.RunCount.Should().Be(1);
            runnable2.IsRunning.Should().BeFalse();
            runnable2.RunCount.Should().Be(1);
        }

        [Theory]
        [InlineData(LimanServiceLifetime.Singleton)]
        [InlineData(LimanServiceLifetime.Application)]
        [InlineData(LimanServiceLifetime.Any)]
        public void Singleton_DisposeCalledAtApplicationEnd(LimanServiceLifetime lifetime)
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), lifetime);
            var application = LimanFactory.CreateApplication(serviceCollection);
            var serviceProvider = application.ServiceProvider;

            // Act
            var service = serviceProvider.GetRequiredService<MyChildService>();
            application.Run();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Disposed && item.Service == service);
        }

        [Fact]
        public void TransientChild_DisposedWhenRemoved()
        {
            // Arrange
            serviceCollection.Add(typeof(MyNode), LimanServiceLifetime.Transient);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var parent = serviceProvider.GetRequiredService<MyNode>();
            var child = parent.CreateChild();
            parent.DeleteChild(child);

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Disposed && item.Service == child);
            lifetimeLog.Should().NotContain(item => item.Action == LifetimeLogAction.Disposed && item.Service == parent);
        }

        [Fact]
        public void TransientChild_DisposedWhenParentIsDeleted()
        {
            // Arrange
            serviceCollection.Add(typeof(MyNode), LimanServiceLifetime.Transient);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var grandParent = serviceProvider.GetRequiredService<MyNode>();
            var parent = grandParent.CreateChild();
            var child = parent.CreateChild();
            grandParent.DeleteChild(parent);

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().NotContain(item => item.Action == LifetimeLogAction.Disposed && item.Service == grandParent);
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Disposed && item.Service == parent);
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Disposed && item.Service == child);
        }

        [Fact]
        public void ScopedService_DisposedWhenScopeIsDeleted()
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), LimanServiceLifetime.Scoped);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);
            var scope = serviceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;

            // Act
            var service = scopedServiceProvider.GetRequiredService<MyChildService>();
            scope.Dispose();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Disposed && item.Service == service);
        }

        [Theory]
        [InlineData(LimanServiceLifetime.Singleton)]
        [InlineData(LimanServiceLifetime.Application)]
        [InlineData(LimanServiceLifetime.Any)]
        [InlineData(LimanServiceLifetime.Transient)]
        public void NonScopedService_NotDisposedWhenScopeIsDeleted(LimanServiceLifetime lifetime)
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), lifetime);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);
            var scope = serviceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;

            // Act
            var service = scopedServiceProvider.GetRequiredService<MyChildService>();
            scope.Dispose();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().NotContain(item => item.Action == LifetimeLogAction.Disposed && item.Service == service);
        }

        public enum LifetimeLogAction
        {
            Construct,
            Initialized,
            Run,
            Disposed
        }

        public class LifetimeLogItem(LifetimeLogAction action, object service)
        {
            public LifetimeLogAction Action { get; } = action;
            public object Service { get; } = service;

            public override string ToString()
            {
                return $"{Action} - {Service.GetType().Name} - {Service}";
            }
        }

        public class LifetimeLog : IEnumerable<LifetimeLogItem>
        {
            private readonly List<LifetimeLogItem> items = [];

            public void Log(LifetimeLogAction action, object service)
            {
                items.Add(new LifetimeLogItem(action, service));
            }

            public int IndexOf(LifetimeLogAction action, object service)
            {
                return items.FindIndex(item => item.Action == action && item.Service == service);
            }

            public IEnumerator<LifetimeLogItem> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
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

        public class MyRunnableService(LifetimeLog log) : MyLifetimeLogger(log), ILimanRunnable
        {
            public void Run()
            {
                log.Log(LifetimeLogAction.Run, this);
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }
        }

        public class MyChildService(LifetimeLog log) : MyLifetimeLogger(log)
        {
        }

        public class MyParentService(LifetimeLog log, MyChildService child) : MyLifetimeLogger(log)
        {
            public MyChildService Child { get; } = child;
        }

        public class MyNode(LifetimeLog log, ILimanServiceProvider serviceProvider) : MyLifetimeLogger(log)
        {
            private static int nextId = 1;
            private readonly ILimanServiceProvider serviceProvider = serviceProvider;

            public int Id { get; } = nextId++;

            public MyNode CreateChild()
            {
                return serviceProvider.GetRequiredService<MyNode>();
            }

            public void DeleteChild(MyNode child)
            {
                serviceProvider.RemoveService(child);
            }

            public override string ToString()
            {
                return $"MyNode{Id}";
            }
        }

        public class MyAsyncRunnable1 : ILimanRunnableAsync
        {
            public bool IsRunning { get; private set; } = false;
            public int RunCount { get; private set; } = 0;

            public async Task Run()
            {
                IsRunning = true;
            }

            public void Stop()
            {
                RunCount++;
                IsRunning = false;
            }
        }

        public class MyAsyncRunnable2 : ILimanRunnableAsync
        {
            public bool IsRunning { get; private set; } = false;
            public int RunCount { get; private set; } = 0;

            public async Task Run()
            {
                IsRunning = true;
            }

            public void Stop()
            {
                RunCount++;
                IsRunning = false;
            }
        }
    }
}
