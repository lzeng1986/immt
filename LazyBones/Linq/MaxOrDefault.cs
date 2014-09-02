using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> keySelector, TResult defaultValue)
        {
            ParamGuard.NotNull(source, "source");
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var comparer = Comparer<TResult>.Default;
                var result = keySelector(enumerator.Current);
                while (enumerator.MoveNext())
                {
                    var tmp = keySelector(enumerator.Current);
                    if (comparer.Compare(tmp, result) > 0)
                    {
                        result = tmp;
                    }
                }
                return result;
            }
            else
            {
                return defaultValue;
            }
        }
        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> keySelector)
        {
            return source.MaxOrDefault(keySelector, default(TResult));
        }
        public static T MaxOrDefault<T>(this IEnumerable<T> source)
        {
            return source.MaxOrDefault(default(T));
        }
        public static T MaxOrDefault<T>(this IEnumerable<T> source, T defaultValue)
        {
            ParamGuard.NotNull(source, "source");
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var comparer = Comparer<T>.Default;
                var result = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var tmp = enumerator.Current;
                    if (comparer.Compare(tmp, result) > 0)
                    {
                        result = tmp;
                    }
                }
                return result;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
