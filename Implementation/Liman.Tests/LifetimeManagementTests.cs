using FluentAssertions;
using System.Collections;

namespace Liman.Tests
{
    public class LifetimeManagementTests
    {
        private ILimanServiceCollection serviceCollection;

        public LifetimeManagementTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
            serviceCollection.Add(typeof(LifetimeLog), ServiceImplementationLifetime.Singleton);
        }

        [Fact]
        public void Child_Initialize_CalledAfterParentConstruction()
        {
            // Arrange
            serviceCollection.Add(typeof(MyChildService), ServiceImplementationLifetime.Any);
            serviceCollection.Add(typeof(MyParentService), ServiceImplementationLifetime.Any);
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
            serviceCollection.Add(typeof(MyChildService), ServiceImplementationLifetime.Application);
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
            serviceCollection.Add(typeof(MyRunnableService), ServiceImplementationLifetime.Application);
            var application = LimanFactory.CreateApplication(serviceCollection);
            var serviceProvider = application.ServiceProvider;

            // Act
            application.Run();

            // Assert
            var lifetimeLog = serviceProvider.GetRequiredService<LifetimeLog>();
            lifetimeLog.Should().ContainSingle(item => item.Action == LifetimeLogAction.Run && item.Service.GetType() == typeof(MyRunnableService));
        }

        [Theory]
        [InlineData(ServiceImplementationLifetime.Singleton)]
        [InlineData(ServiceImplementationLifetime.Application)]
        [InlineData(ServiceImplementationLifetime.Any)]
        public void Singleton_DisposeCalledAtApplicationEnd(ServiceImplementationLifetime lifetime)
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
            serviceCollection.Add(typeof(MyNode), ServiceImplementationLifetime.Transient);
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
            serviceCollection.Add(typeof(MyNode), ServiceImplementationLifetime.Transient);
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

        public enum LifetimeLogAction
        {
            Construct,
            Initialized,
            Run,
            Disposed
        }

        public class LifetimeLogItem
        {
            public LifetimeLogItem(LifetimeLogAction action, object service)
            {
                Action = action;
                Service = service;
            }

            public LifetimeLogAction Action { get; }
            public object Service { get; }
        }

        public class LifetimeLog: IEnumerable<LifetimeLogItem>
        {
            private readonly List<LifetimeLogItem> items = new();

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

        public class MyLifetimeLogger: IInitializable, IDisposable
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
            }
        }

        public class MyRunnableService : MyLifetimeLogger, IRunnable
        {
            public MyRunnableService(LifetimeLog log) : base(log)
            {
            }

            public void Run()
            {
                log.Log(LifetimeLogAction.Run, this);
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }
        }

        public class MyChildService : MyLifetimeLogger
        {
            public MyChildService(LifetimeLog log) : base(log)
            {
            }
        }

        public class MyParentService: MyLifetimeLogger
        {
            public MyParentService(LifetimeLog log, MyChildService child) : base(log)
            {
                Child = child;
            }

            public MyChildService Child { get; }
        }

        public class MyNode : MyLifetimeLogger
        {
            private readonly ILimanServiceProvider serviceProvider;

            public MyNode(LifetimeLog log, ILimanServiceProvider serviceProvider) : base(log)
            {
                this.serviceProvider = serviceProvider;
            }

            public MyNode CreateChild()
            {
                return serviceProvider.GetRequiredService<MyNode>();
            }

            public void DeleteChild(MyNode child)
            {
                serviceProvider.RemoveService(child);
            }
        }
    }
}
