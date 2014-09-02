using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<KeyValuePair<int, T>> Index<T>(this IEnumerable<T> source)
        {
            return source.Index(0);
        }
        public static IEnumerable<KeyValuePair<int, T>> Index<T>(this IEnumerable<T> source, int seed)
        {
            return source.Select((t, i) => new KeyValuePair<int, T>(i + seed, t));
        }
    }
}
