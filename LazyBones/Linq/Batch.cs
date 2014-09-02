using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 以固定大小的分割序列
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            ParamGuard.NotNull(source, "source");
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", "size必须大于0");
            var list = new List<T>(size);
            using (var enumerator = source.GetEnumerator())
            {
                while (true)
                {
                    for (var i = 0; i < size && enumerator.MoveNext(); i++)
                        list.Add(enumerator.Current);
                    if (list.Count == 0)
                        yield break;
                    yield return list;
                    list.Clear();
                }
            }
        }
    }
}
