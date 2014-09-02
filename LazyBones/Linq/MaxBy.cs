using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 根据某一键值返回最大元素
        /// </summary>
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.MaxBy(keySelector, Comparer<TKey>.Default);
        }
        /// <summary>
        /// 根据某一键值返回最大元素
        /// </summary>
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            ParamGuard.NotNull(source, "source");
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var result = enumerator.Current;
                var cmp = keySelector(result);
                while (enumerator.MoveNext())
                {
                    var tmp = keySelector(enumerator.Current);
                    if (comparer(tmp, cmp) > 0)
                    {
                        result = enumerator.Current;
                        cmp = tmp;
                    }
                }
                return result;
            }
            else
            {
                throw new InvalidOperationException("序列不包括任何元素");
            }
        }
        /// <summary>
        /// 根据某一键值返回最大元素
        /// </summary>
        public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return source.MaxBy(keySelector, comparer.Compare);
        }
    }
}
