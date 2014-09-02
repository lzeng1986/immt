using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 以指定间隔提取值
        /// </summary>
        public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int step)
        {
            return source.Where((t, i) => i % step == 0);
        }
    }
}
