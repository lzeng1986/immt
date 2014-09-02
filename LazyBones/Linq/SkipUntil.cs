using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<T> SkipUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.SkipUntil((t, i) => predicate(t));
        }
        public static IEnumerable<T> SkipUntil<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            ParamGuard.NotNull(source, "source");
            using (var enumerator = source.GetEnumerator())
            {
                bool hasMore;
                var ind = 0;
                while ((hasMore = enumerator.MoveNext()) && !predicate(enumerator.Current, ind))
                    ind++;
                if (hasMore)
                {
                    do
                    {
                        yield return enumerator.Current;
                    }
                    while (enumerator.MoveNext());
                }
            }
        }
    }
}
