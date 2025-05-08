using System.Reflection;

namespace Liman.Implementation.ServiceCollections
{
    internal class LimanServiceImplementation : ILimanServiceImplementation
    {
        public LimanServiceImplementation(Type type, LimanImplementationLifetime lifetime, Delegate? factoryMethod)
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
                if (parameter.GetCustomAttribute<LimanNoInjectionAttribute>() != null)
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
                    throw new LimanException($"Some 'NoInjection' parameters are NOT at the end of the constructor for service implementation '{type.GetReadableName()}'");
                }
            }

            ServiceParameters = usedServices.AsReadOnly();
            CustomParameters = customParameters;
        }

        public Type Type { get; }
        public LimanImplementationLifetime Lifetime { get; }
        public IReadOnlyList<Type> ServiceParameters { get; }
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
                    ?? throw new LimanException($"Factory method '{FactoryMethod.Name}' did not return an instance.");
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
                    ?? throw new LimanException($"Could not find a suitable constructor for service implementation '{type}'.");
            }
        }

        public override string ToString()
        {
            return Type.GetReadableName();
        }
    }
}
