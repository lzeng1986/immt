using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 根据指定comparer操作进行Distinct操作
        /// </summary>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer)
        {
            return source.Distinct(CmpFactory.Create<T>(comparer));
        }
    }
}
