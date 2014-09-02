using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 成对操作
        /// </summary>
        public static IEnumerable<TResult> Pair<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> func)
        {
            ParamGuard.NotNull(source, "source");
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;
                var prev = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    yield return func(prev, current);
                    prev = current;
                }
            }
        }
    }
}
