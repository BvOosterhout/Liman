using Liman.Implementation.ServiceImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liman.Implementation
{
    internal static class ExceptionHelper
    {
        public static string CreateCircularDependencyMessage(IEnumerable<LimanServiceImplementation> creationsInProgress, LimanServiceImplementation circularService)
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
