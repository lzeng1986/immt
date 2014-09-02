using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<T> Pipe<T>(this IEnumerable<T> source, Action<T> action)
        {
            ParamGuard.NotNull(source, "source");
            foreach (var v in source)
            {
                action(v);
                yield return v;
            }
        }
    }
}
