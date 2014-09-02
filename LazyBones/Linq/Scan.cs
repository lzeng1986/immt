using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 以指定操作对序列进行扫描
        /// </summary>
        public static IEnumerable<T> Scan<T>(this IEnumerable<T> source, Func<T, T, T> func)
        {
            return source.Scan(t => t, func);
        }
        public static IEnumerable<TResult> Scan<T, TResult>(this IEnumerable<T> source, Func<T, TResult> resultSelector, Func<TResult, TResult, TResult> func)
        {
            ParamGuard.NotNull(source, "source");
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;
                var accumulator = resultSelector(enumerator.Current);
                while (enumerator.MoveNext())
                {
                    yield return accumulator;
                    var current = resultSelector(enumerator.Current);
                    accumulator = func(accumulator, current);
                }
            }
        }
    }
}
