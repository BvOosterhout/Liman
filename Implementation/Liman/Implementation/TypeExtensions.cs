using System.Text;

namespace Liman.Implementation
{
    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetImplementationTypes(this Type serviceType)
        {
            return serviceType.Assembly.GetTypes().Where(x => !x.IsAbstract && x.IsAssignableTo(serviceType));
        }

        public static IEnumerable<Type> GetImplementedTypes(this Type implementationType)
        {
            var baseType = implementationType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }

            foreach (var @interface in implementationType.GetInterfaces())
            {
                yield return @interface;
            }
        }

        public static Type[] GetGenericArguments(this Type implementationType, Type genericType)
        {
            var implementedGenericType = implementationType.GetImplementedTypes().Prepend(implementationType)
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
            return implementedGenericType.GetGenericArguments();
        }

        public static bool ImplementsGenericType(this Type implementationType, Type genericType)
        {
            var implementedGenericType = implementationType.GetImplementedTypes().Prepend(implementationType)
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
            return implementedGenericType != null;
        }

        public static string GetReadableName(this Type type)
        {
            if (type.IsGenericType)
            {
                var stringBuilder = new StringBuilder();
                int arityIndex = type.Name.IndexOf('`');
                if (arityIndex == -1) throw new InvalidOperationException();

                stringBuilder.Append(type.Name.AsSpan(0, arityIndex));
                stringBuilder.Append('<');

                bool isFirst = true;
                foreach (var argument in type.GetGenericArguments())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        stringBuilder.Append(", ");
                    }

                    stringBuilder.Append(argument.Name);
                }
                stringBuilder.Append('>');
                return stringBuilder.ToString();
            }
            else
            {
                return type.Name;
            }
        }
    }
}
