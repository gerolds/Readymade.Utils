using System.Collections.Generic;

namespace App.Core.Utils
{
    public static class DictionaryExtensions
    {
        public static bool SafeAdd<K, T>(this IDictionary<K, HashSet<T>> dict, K key, T value)
        {
            if (!dict.TryGetValue(key, out var collection))
            {
                dict[key] = collection = new HashSet<T>();
            }

            return collection.Add(value);
        }

        public static void SafeAdd<K, T>(this IDictionary<K, List<T>> dict, K key, T value)
        {
            if (!dict.TryGetValue(key, out var collection))
            {
                dict[key] = collection = new List<T>();
            }

            collection.Add(value);
        }

        public static void SafeAdd<K, T>(this IDictionary<K, Stack<T>> dict, K key, T value)
        {
            if (!dict.TryGetValue(key, out var collection))
            {
                dict[key] = collection = new Stack<T>();
            }

            collection.Push(value);
        }

        public static void SafeAdd<K, T>(this IDictionary<K, Queue<T>> dict, K key, T value)
        {
            if (!dict.TryGetValue(key, out var collection))
            {
                dict[key] = collection = new Queue<T>();
            }

            collection.Enqueue(value);
        }
    }
}