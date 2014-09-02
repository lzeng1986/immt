using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<T> Preconcat<T>(this IEnumerable<T> source, T head)
        {
            return head.Concat(source);
        }
        public static IEnumerable<T> Preconcat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            return second.Concat(first);
        }
    }
}
