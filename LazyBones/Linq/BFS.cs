using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 实现广度优先搜索
        /// </summary>
        public static IEnumerable<T> BFS<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> generator)
        {
            ParamGuard.NotNull(source, "source");
            var queue = new Queue<T>(source);
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                generator(v).ForEach(queue.Enqueue);
                yield return v;
            }
        }
    }
}
