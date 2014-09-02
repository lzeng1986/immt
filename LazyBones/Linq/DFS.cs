using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 实现深度优先搜索
        /// </summary>
        public static IEnumerable<T> DFS<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> generator)
        {
            return source.SelectMany(v => v.Concat(DFS(generator(v), generator)));
        }
    }
}
