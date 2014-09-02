using System;
using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class LinqEx
    {
        /// <summary>
        /// 将两个序列合并成一个，返回序列长度与其中较短序列长度相等
        /// </summary>
        public static IEnumerable<TResult> ZipIn35<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
        {
            var eFirst = first.GetEnumerator();
            var eSecond = second.GetEnumerator();
            while (eFirst.MoveNext() && eSecond.MoveNext())
            {
                yield return selector(eFirst.Current, eSecond.Current);
            }
        }
    }
}
