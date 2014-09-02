using System;
using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static int IndexOf<T>(this IEnumerable<T> source, T target)
        {
            return source.IndexOf(target, EqualityComparer<T>.Default);
        }
        public static int IndexOf<T>(this IEnumerable<T> source, T target, IEqualityComparer<T> comparer)
        {
            return source.IndexOf(t => comparer.Equals(t, target));
        }
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var index = 0;
            foreach (var c in source)
            {
                if (predicate(c))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
