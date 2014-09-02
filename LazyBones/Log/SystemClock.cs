using System;

namespace LazyBones.Log
{
    /// <summary>
    /// 系统时间
    /// </summary>
    internal static class SystemClock
    {
        /// <summary>
        /// 当前系统时间
        /// </summary>
        public static DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
