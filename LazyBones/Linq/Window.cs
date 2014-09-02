using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 以固定大小的窗口扫描序列
        /// </summary>
        public static IEnumerable<TResult> Window<T, TResult>(this IEnumerable<T> source, int size, Func<IEnumerable<T>, TResult> func)
        {
            ParamGuard.NotNull(source, "source");
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size", "size必须大于0");

            var list = new List<T>(size);
            using (var enumerator = source.GetEnumerator())
            {
                for (var i = 0; i < size && enumerator.MoveNext(); i++)
                    list.Add(enumerator.Current);

                if (list.Count == 0)
                    yield break;

                yield return func(list);

                if (list.Count == size)
                {
                    while (enumerator.MoveNext())
                    {
                        list.RemoveAt(0);
                        list.Add(enumerator.Current);
                        yield return func(list);
                    }
                }
            }
        }
    }
}
