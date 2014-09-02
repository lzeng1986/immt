using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 交叉合并两个序列
        /// </summary>
        public static IEnumerable<TResult> MergeCross<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
        {
            ParamGuard.NotNull(first, "first");
            ParamGuard.NotNull(second, "second");
            return from a in first
                   from b in second
                   select selector(a, b);
        }
    }
}
