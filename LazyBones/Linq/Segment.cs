using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 截取序列的一段
        /// </summary>
        public static IEnumerable<T> Segment<T>(this IEnumerable<T> source, int from, int count)
        {
            return source.Skip(from).Take(count);
        }
    }
}
