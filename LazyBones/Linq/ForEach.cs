using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ParamGuard.NotNull(source, "source");
            foreach (var v in source)
                action(v);
        }
        public static void ForEach<T>(this T arg, IEnumerable<Action<T>> actions)
        {
            ParamGuard.NotNull(actions, "actions");
            foreach (var action in actions)
                action(arg);
        }
        public static void ForEach<T>(this IEnumerable<T> source, IEnumerable<Action<T>> actions)
        {
            ParamGuard.NotNull(source, "source");
            ParamGuard.NotNull(actions, "actions");
            foreach (var action in actions)
                foreach (var v in source)
                    action(v);
        }
    }
}
