using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T tail)
        {
            foreach (var v in source)
                yield return v;
            yield return tail;
        }
        public static IEnumerable<T> Concat<T>(this T head, IEnumerable<T> source)
        {
            yield return head;
            foreach (var v in source)
                yield return v;
        }
    }
}
