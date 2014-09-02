using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 根据某一键值返回最小元素
        /// </summary>
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return source.MinBy(keySelector, Comparer<TKey>.Default);
        }
        /// <summary>
        /// 根据某一键值返回最小元素
        /// </summary>
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            return source.MinBy(keySelector, CmpFactory.Create<TKey>(comparer));
        }
        /// <summary>
        /// 根据某一键值返回最小元素
        /// </summary>
        public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            var result = source.First();
            var cmp = keySelector(result);
            foreach (T d in source.Skip(1))
            {
                var tmp = keySelector(d);
                if (comparer.Compare(tmp, cmp) < 0)
                {
                    result = d;
                    cmp = tmp;
                }
            }
            return result;
        }
    }
}
