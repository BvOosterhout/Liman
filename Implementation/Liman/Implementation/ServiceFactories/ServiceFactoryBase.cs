using Microsoft.Extensions.DependencyInjection;

namespace Liman.Implementation.ServiceFactories
{
    internal abstract class ServiceFactoryBase : IServiceFactory
    {
        private readonly IServiceFactoryProvider serviceFactoryProvider;

        public ServiceFactoryBase(
            IServiceFactoryProvider serviceFactoryProvider,
            ILimanServiceImplementation implementationType)
        {
            this.serviceFactoryProvider = serviceFactoryProvider;
            ImplementationType = implementationType;
        }

        public ILimanServiceImplementation ImplementationType { get; }
        public abstract ServiceImplementationLifetime Lifetime { get; }
        public abstract object? Get(IServiceScope? scope, object?[] customArguments);

        public virtual void RegisterUser(object user, object dependency)
        {
        }

        protected virtual IServiceFactory[] GetDependencyFactories()
        {
            return serviceFactoryProvider.GetUsedServices(ImplementationType);
        }

        protected object CreateInstance(IServiceScope? scope, object?[] customArguments)
        {
            if (customArguments.Length != ImplementationType.CustomParameters.Count)
            {
                var expectedTypes = string.Join(", ", ImplementationType.CustomParameters.Select(x => x.GetReadableName()));
                var receivedTypes = string.Join(", ", ImplementationType.CustomParameters.Select(x => x == null ? "null" : x.GetType().GetReadableName()));

                throw new LimanException($"Invalid custom arguments for implementation '{ImplementationType}'. Expected types '{expectedTypes}', received '{receivedTypes}'");
            }

            serviceFactoryProvider.PrepareCreation(ImplementationType);

            var argumentFactories = GetDependencyFactories();
            var arguments = new object?[argumentFactories.Length + customArguments.Length];

            int index = 0;
            foreach (var factory in argumentFactories)
            {
                arguments[index] = factory.Get(scope, []);
                index++;
            }

            if (customArguments.Length > 0)
            {
                customArguments.CopyTo(arguments, index);
            }

            var instance = ImplementationType.CreateInstance(arguments);

            StoreInstance(instance, scope, customArguments);

            serviceFactoryProvider.FinishCreation(ImplementationType, instance);

            index = 0;
            foreach (var factory in argumentFactories)
            {
                var depedency = arguments[index];

                if (depedency != null)
                {
                    factory.RegisterUser(instance, depedency);
                }

                index++;
            }

            return instance;
        }

        protected virtual void StoreInstance(object instance, IServiceScope? scope, object?[] customArguments)
        {
        }
    }
}
