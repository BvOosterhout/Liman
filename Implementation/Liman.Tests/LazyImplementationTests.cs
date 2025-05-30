﻿using FluentAssertions;

namespace Liman.Tests
{
    public class LazyImplementationTests
    {
        private ILimanServiceCollection serviceCollection;

        public LazyImplementationTests()
        {
            serviceCollection = LimanFactory.CreateServiceCollection();
        }

        [Fact]
        public void LazyService_PreventsCircularDependency()
        {
            // Arrange
            serviceCollection.Add(typeof(MyParentService), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(MyChildService), LimanServiceLifetime.Any);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var lazyService = serviceProvider.GetRequiredService<MyChildService>();
            var parentService = lazyService.GetParentService();

            // Assert
            lazyService.Should().NotBeNull();
            parentService.Should().NotBeNull();
        }

        [Fact]
        public void LazyImplementationCollection_PreventsCircularDependency()
        {
            // Arrange
            serviceCollection.Add(typeof(MyCollectionService), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(MyCollectionItem1), LimanServiceLifetime.Any);
            serviceCollection.Add(typeof(MyCollectionItem2), LimanServiceLifetime.Any);
            var serviceProvider = LimanFactory.CreateServiceProvider(serviceCollection);

            // Act
            var collectionService = serviceProvider.GetRequiredService<MyCollectionService>();
            var itemServices = collectionService.GetItems();

            // Assert
            itemServices.Should().ContainSingle(x => x is MyCollectionItem1);
            itemServices.Should().ContainSingle(x => x is MyCollectionItem2);
        }

        public class MyParentService
        {
            private MyChildService childService;

            public MyParentService(MyChildService childService)
            {
                this.childService = childService;
            }
        }

        public class MyChildService
        {
            private Lazy<MyParentService> parentService;

            public MyChildService(Lazy<MyParentService> parentService)
            {
                this.parentService = parentService;
            }

            public MyParentService GetParentService()
            {
                return parentService.Value;
            }
        }

        public class MyCollectionService
        {
            private readonly ILimanImplementationCollection<IMyCollectionItem> items;

            public MyCollectionService(ILimanImplementationCollection<IMyCollectionItem> items)
            {
                this.items = items;
            }

            public List<IMyCollectionItem> GetItems()
            {
                return items.ToList();
            }
        }

        public interface IMyCollectionItem
        {
        }

        public class MyCollectionItem1 : IMyCollectionItem
        {
        }

        public class MyCollectionItem2 : IMyCollectionItem
        {
        }
    }
}
