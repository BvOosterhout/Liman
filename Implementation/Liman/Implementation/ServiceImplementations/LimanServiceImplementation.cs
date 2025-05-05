using System.Reflection;

namespace Liman.Implementation.ServiceImplementations
{
    internal class LimanServiceImplementation
    {
        public LimanServiceImplementation(Type type, ServiceImplementationLifetime lifetime, Delegate? factoryMethod)
        {
            Type = type;
            Lifetime = lifetime;

            FactoryMethod = factoryMethod?.Method ?? GetConstructor(type);
            FactoryMethodInstance = factoryMethod?.Target;

            var usedServices = new List<Type>();
            var customParameters = new List<Type>();
            bool unInjectable = false;

            foreach (var parameter in FactoryMethod.GetParameters())
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
            CustomParameters = customParameters;
        }

        public Type Type { get; }
        public ServiceImplementationLifetime Lifetime { get; }
        public IReadOnlyList<Type> UsedServices { get; }
        public IReadOnlyList<Type> CustomParameters { get; }
        public MethodBase FactoryMethod { get; }
        public object? FactoryMethodInstance { get; }

        public object CreateInstance(object?[] arguments)
        {
            if (FactoryMethod is ConstructorInfo constructor)
            {
                return constructor.Invoke(arguments);
            }
            else
            {
                return FactoryMethod.Invoke(FactoryMethodInstance, arguments)
                    ?? throw new NullReferenceException();
            }
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
