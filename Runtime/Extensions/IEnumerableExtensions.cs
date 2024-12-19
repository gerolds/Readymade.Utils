using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Readymade.Utils
{
    /// <summary>
    /// Extension methods acting on <see cref="IEnumerable{T}"/> instances.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Performs the specified action on each element of the <see name="IEnumerable{T}" />.
        /// </summary>
        /// <param name="source">The <see name="IEnumerable{T}" /> to iterate.</param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see name="IEnumerable{T}" />.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int i = 0;
            foreach (T item in source)
            {
                action(item, i);
                i++;
            }
        }

        public static void ForEachInCopy<T>(this IEnumerable<T> source, Action<T> action)
        {
            List<T> buffer = ListPool<T>.Get();
            try
            {
                foreach (T item in source)
                {
                    buffer.Add(item);
                }

                foreach (var item in buffer)
                {
                    action(item);
                }
            }
            finally
            {
                ListPool<T>.Release(buffer);
            }
        }

        public static void ForEachInCopy<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            List<T> buffer = ListPool<T>.Get();
            try
            {
                foreach (T item in source)
                {
                    buffer.Add(item);
                }

                for (var i = 0; i < buffer.Count; i++)
                {
                    action(buffer[i], i);
                }
            }
            finally
            {
                ListPool<T>.Release(buffer);
            }
        }
    }
}