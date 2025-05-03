using Liman.Implementation.ServiceImplementations;
using System.Text;

namespace Liman
{
    internal class CircularDependencyException : Exception
    {
        public CircularDependencyException(List<LimanServiceImplementation> creationsInProgress, LimanServiceImplementation circularService)
            : base(CreateMessage(creationsInProgress, circularService))
        {
        }

        private static string CreateMessage(List<LimanServiceImplementation> creationsInProgress, LimanServiceImplementation circularService)
        {
            var userService = creationsInProgress.Last();
            var builder = new StringBuilder();

            builder.AppendLine($"Failed to inject service '{userService}'; Circular dependency detected for type '{circularService}'");
            builder.AppendLine($"Injection order: ");

            foreach (var injectionType in creationsInProgress)
            {
                builder.AppendLine($" - '{injectionType}'");
            }

            return builder.ToString();
        }
    }
}
