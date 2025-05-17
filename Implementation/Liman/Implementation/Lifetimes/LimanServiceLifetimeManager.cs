using System.Collections.Immutable;

namespace Liman.Implementation.Lifetimes
{
    [LimanService(LimanServiceLifetime.Singleton)]
    internal class LimanServiceLifetimeManager(ILimanServiceCollection serviceCollection) : ILimanServiceLifetimeManager
    {
        private readonly List<object> singletons = [];
        private readonly Dictionary<object, List<object>> usersByTransient = [];
        private readonly Dictionary<object, List<object>> transientsByUser = [];
        private readonly Dictionary<Type, bool> needsCleanupByType = [];

        public void AddSingleton(object singleton)
        {
            if (NeedsCleanup(singleton.GetType()))
            {
                singletons.Add(singleton);
            }
        }

        public bool AddTransientDependency(object user, object transient)
        {
            if (NeedsCleanup(transient.GetType()))
            {
                usersByTransient.AddItem(transient, user);
                transientsByUser.AddItem(user, transient);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteTransientDependency(object user, object dependency)
        {
            if (usersByTransient.TryGetValue(dependency, out var users) && users.Remove(user))
            {
                transientsByUser.RemoveItem(user, dependency);

                if (users.Count == 0)
                {
                    usersByTransient.Remove(dependency);
                    Delete(dependency);
                }
            }
        }

        public void DeleteAllServices()
        {
            // Singletons
            foreach (var singleton in singletons.Reverse<object>())
            {
                Delete(singleton);
            }

            singletons.Clear();

            // Transients
            var transients = transientsByUser.Keys.ToImmutableArray();

            foreach (var transient in transients)
            {
                Delete(transient);
            }

            transientsByUser.Clear();
            usersByTransient.Clear();
        }

        public void Delete(object implementation)
        {
            if (implementation is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (usersByTransient.TryGetValue(implementation, out var users))
            {
                foreach (var user in users.ToImmutableArray())
                {
                    transientsByUser.RemoveItem(user, implementation);
                }

                usersByTransient.Remove(implementation);
            }

            if (transientsByUser.TryGetValue(implementation, out var transients))
            {
                foreach (var transient in transients.ToImmutableArray())
                {
                    DeleteTransientDependency(implementation, transient);
                }
            }
        }

        private bool NeedsCleanup(Type type)
        {
            if (needsCleanupByType.TryGetValue(type, out var result))
            {
                return result;
            }

            if (type.IsAssignableTo(typeof(IDisposable)))
            {
                result = true;
            }
            else if (serviceCollection.TryGetSingle(type, out var implementationType))
            {
                foreach (var service in implementationType.ServiceParameters)
                {
                    if (service.IsAssignableTo(typeof(IServiceProvider)))
                    {
                        result = true;
                        break;
                    }
                    else if (NeedsCleanup(service))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            needsCleanupByType.Add(type, result);

            return result;
        }
    }
}
