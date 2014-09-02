using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 向左旋转一个序列
        /// </summary>
        public static IEnumerable<T> RotateLeft<T>(this IEnumerable<T> source, int offset)
        {
            ParamGuard.NotNull(source, "source");
            var list = source as IList<T>;//如果source实现IList接口，则直接只用原序列，否则生成新的List
            list = list ?? new List<T>(source);
            var count = list.Count;
            if (count == 0)
                yield break;
            offset = offset % count;
            for (var i = offset; i < count; i++)
                yield return list[i];
            for (var i = 0; i < offset; i++)
                yield return list[i];
        }

        /// <summary>
        /// 向右旋转一个序列
        /// </summary>
        public static IEnumerable<T> RotateRight<T>(this IEnumerable<T> source, int offset)
        {
            ParamGuard.NotNull(source, "source");
            var list = source as IList<T>;
            list = list ?? new List<T>(source);
            var count = list.Count;
            if (count == 0)
                yield break;
            offset = offset % count;
            var start = count - offset;
            for (var i = start; i < count; i++)
                yield return list[i];
            for (var i = 0; i < start; i++)
                yield return list[i];
        }
    }
}
