namespace Liman.Implementation
{
    internal static class DictionaryExtensions
    {
        public static void AddItem<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem itemToAdd)
            where TKey : notnull
        {
            if (!dictionary.TryGetValue(key, out var items))
            {
                items = new List<TItem>();
                dictionary.Add(key, items);
            }

            items.Add(itemToAdd);
        }

        public static bool RemoveItem<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem itemToAdd)
            where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out var items))
            {
                var result = items.Remove(itemToAdd);

                if (items.Count == 0)
                {
                    dictionary.Remove(key);
                }

                return result;
            }
            else
            {
                return false;
            }
        }
    }
}
