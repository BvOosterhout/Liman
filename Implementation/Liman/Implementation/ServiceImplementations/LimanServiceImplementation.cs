using System.Reflection;

namespace Liman.Implementation.ServiceImplementations
{
    internal class LimanServiceImplementation
    {
        private readonly MethodBase factoryMethod;
        private readonly object? factoryMethodInstance;

        public LimanServiceImplementation(Type type, ServiceImplementationLifetime lifetime, Delegate? factoryMethod)
        {
            Type = type;
            Lifetime = lifetime;

            if (factoryMethod != null)
            {
            }

            this.factoryMethod = factoryMethod?.Method ?? GetConstructor(type);
            factoryMethodInstance = factoryMethod?.Target;

            var usedServices = new List<Type>();
            var customParameters = new List<Type>();
            bool unInjectable = false;

            foreach (var parameter in this.factoryMethod.GetParameters())
            {
                if (parameter.GetCustomAttribute<NoInjectionAttribute>() != null)
                {
                    customParameters.Add(parameter.ParameterType);
                    unInjectable = true;
                }
                else if (!unInjectable)
                {
                    usedServices.Add(parameter.ParameterType);
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            UsedServices = usedServices.AsReadOnly();
        }

        public Type Type { get; }
        public ServiceImplementationLifetime Lifetime { get; }
        public IReadOnlyList<Type> UsedServices { get; }
        public IReadOnlyList<Type> CustomParameters { get; }

        public object CreateInstance(object?[] arguments)
        {
            return factoryMethod.Invoke(factoryMethodInstance, arguments)
                ?? throw new NullReferenceException();
        }

        private static MethodBase GetConstructor(Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length == 1)
            {
                return constructors[0];
            }
            else
            {
                return constructors.FirstOrDefault(x => x.GetParameters().Length == 0)
                    ?? throw new ArgumentException($"Could not find a suitable constructor for type '{type}'.");
            }
        }

        public override string ToString()
        {
            return Type.GetReadableName();
        }
    }
}
