using System;
using System.Collections.Generic;

namespace Kairou
{
    internal static class ListExtensions
    {
        public static void Move<T>(this List<T> list, int fromIndex, int toIndex)
        {
            T item = list[fromIndex];
            list.RemoveAt(fromIndex);
            list.Insert(toIndex, item);
        }

        public static bool HasElementAt<T>(this IReadOnlyList<T> list, int index) => index >= 0 && index < list.Count;

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], item)) return i;
            }
            return -1;
        }
    }
}