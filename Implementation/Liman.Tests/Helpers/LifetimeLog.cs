using System.Collections;
using static Liman.Tests.LifetimeManagementTests;

namespace Liman.Tests.Helpers
{
    public class LifetimeLog : IEnumerable<LifetimeLogItem>
    {
        private readonly List<LifetimeLogItem> items = [];

        public void Log(LifetimeLogAction action, object service)
        {
            items.Add(new LifetimeLogItem(action, service));
        }

        public int IndexOf(LifetimeLogAction action, object service)
        {
            return items.FindIndex(item => item.Action == action && item.Service == service);
        }

        public IEnumerator<LifetimeLogItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
