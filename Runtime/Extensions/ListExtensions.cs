using System.Collections.Generic;

namespace Readymade.Building
{
    public static class ListExtensions
    {
        public static bool TryPop<T>(this IList<T> stack, out T value)
        {
            if (stack.Count > 0)
            {
                value = stack[^1];
                stack.RemoveAt(stack.Count - 1);
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryPeek<T>(this IList<T> stack, out T value)
        {
            if (stack.Count > 0)
            {
                value = stack[^1];
                return true;
            }

            value = default;
            return false;
        }

        public static void Push<T>(this IList<T> stack, T value) => stack.Add(value);
    }
}