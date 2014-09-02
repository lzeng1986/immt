using System;
using System.Collections.Generic;
using System.Text;
using LazyBones.Utils;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 将序列合并成一个字符串，采用Environment.NewLine进行拼接
        /// </summary>
        public static string JoinToString<T>(this IEnumerable<T> source)
        {
            return source.JoinToString(Environment.NewLine);
        }
        /// <summary>
        /// 将序列合并成一个字符串
        /// </summary>
        public static string JoinToString<T>(this IEnumerable<T> source, string separator)
        {
            return source.JoinToString((sb, v) =>
            {
                sb.Append(v);
                sb.Append(separator);
            });
        }
        /// <summary>
        /// 将序列已指定形式合并成一个字符串
        /// </summary>
        public static string JoinToString<T>(this IEnumerable<T> source, Action<StringBuilder, T> func)
        {
            ParamGuard.NotNull(source, "source");
            var sb = new StringBuilder();
            foreach (var v in source)
            {
                func(sb, v);
            }
            return sb.ToString();
        }
    }
}
