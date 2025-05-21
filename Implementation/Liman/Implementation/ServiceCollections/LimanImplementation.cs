using System.Reflection;

namespace Liman.Implementation.ServiceCollections
{
    internal class LimanImplementation : ILimanImplementation
    {
        public LimanImplementation(Type? type, LimanServiceLifetime lifetime, Delegate? factoryMethod)
        {
            Type = type;
            Lifetime = lifetime;

            FactoryMethod = factoryMethod;
            if (FactoryMethod == null) Constructor = GetConstructor(type ?? throw new ArgumentNullException(nameof(type)));

            var usedServices = new List<Type>();
            var customParameters = new List<Type>();
            bool unInjectable = false;

            var parameters = factoryMethod?.Method?.GetParameters() ?? Constructor?.GetParameters() ?? [];

            foreach (var parameter in parameters)
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
                    throw new LimanException($"Some 'NoInjection' parameters are NOT at the end of the constructor for service implementation '{this}'");
                }
            }

            ServiceParameters = usedServices.AsReadOnly();
            CustomParameters = customParameters;
        }

        public Type? Type { get; }
        public LimanServiceLifetime Lifetime { get; }
        public IReadOnlyList<Type> ServiceParameters { get; }
        public IReadOnlyList<Type> CustomParameters { get; }

        public Delegate? FactoryMethod { get; }
        public ConstructorInfo? Constructor { get; }

        public object CreateInstance(object?[] arguments)
        {
            if (Constructor != null)
            {
                return Constructor.Invoke(arguments);
            }
            else if (FactoryMethod != null)
            {
                return FactoryMethod.DynamicInvoke(arguments)
                    ?? throw new LimanException($"Factory method '{FactoryMethod.Method.Name}' for type '{Type}', did not return an instance.");
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static ConstructorInfo GetConstructor(Type type)
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
            if (Type != null)
            {
                return Type.GetReadableName();
            }
            else
            {
                return FactoryMethod!.Method.Name;
            }
        }
    }
}
