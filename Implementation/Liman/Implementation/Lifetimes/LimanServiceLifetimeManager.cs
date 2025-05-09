using System.Collections.Immutable;

namespace Liman.Implementation.Lifetimes
{
    [LimanService(LimanServiceLifetime.Singleton)]
    internal class LimanServiceLifetimeManager : ILimanServiceLifetimeManager
    {
        private readonly ILimanServiceCollection serviceCollection;
        private readonly List<object> singletons = new();
        private readonly Dictionary<object, List<object>> usersByTransient = new();
        private readonly Dictionary<object, List<object>> transientsByUser = new();
        private readonly Dictionary<Type, bool> needsCleanupByType = new();


        public LimanServiceLifetimeManager(ILimanServiceCollection serviceImplementationRepository)
        {
            this.serviceCollection = serviceImplementationRepository;
        }

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

        public void DeleteTransientDependency(object user, object transient)
        {
            if (usersByTransient.TryGetValue(transient, out var users) && users.Remove(user))
            {
                transientsByUser.RemoveItem(user, transient);

                if (users.Count == 0)
                {
                    usersByTransient.Remove(transient);
                    Delete(transient);
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
