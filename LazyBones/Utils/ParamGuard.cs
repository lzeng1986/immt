using System;

namespace LazyBones.Utils
{
    /// <summary>
    /// 辅助类，用于检测参数是否符合要求
    /// </summary>
    public static class ParamGuard
    {
        /// <summary>
        /// 检测参数是否为null，如果为空，则抛出异常<see cref="ArgumentNullException"/>
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="arg">检测的参数</param>
        /// <param name="parameterName">参数名称</param>
        public static void NotNull<T>(T arg, string parameterName)
            where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
        /// <summary>
        /// 检测参数是否在范围之类，不包含边界
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="arg">检测的参数</param>
        /// <param name="min">参数下界</param>
        /// <param name="max">参数上届</param>
        /// <param name="parameterName">参数名称</param>
        public static void InRange<T>(T arg, T min, T max, string parameterName)
            where T:IComparable<T>
        {
            if (arg.CompareTo(min) < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "值不得小于" + min.ToString());
            }
            if (arg.CompareTo(max) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "值不得大于" + max.ToString());
            }
        }

    }
}
