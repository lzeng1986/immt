using System;
using LazyBones.Log.Core;
using LazyBones.Utils;

namespace LazyBones.Log
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public class LogLevel
    {
        int order;
        string name;
        string value;
        private LogLevel(int order, string name, string value)
        {
            this.order = order;
            this.name = name;
            this.value = value;
        }
        /// <summary>
        /// 获取日志级别序号，序号越大日志级别越高
        /// </summary>
        public int Order
        {
            get { return order; }
        }
        /// <summary>
        /// 获取日志级别的名称
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        /// <summary>
        /// 将此<see cref="LogLevel"/>转换为可读字符串
        /// </summary>
        /// <returns>表示此<see cref="LogLevel"/>的字符串</returns>
        public override string ToString()
        {
            return name;
        }
        /// <summary>
        /// 获取日志级别的哈希值
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode()
        {
            return order;
        }
        /// <summary>
        /// 指定此<see cref="LogLevel"/>是否与指定<see cref="object"/>有相同的日志级别
        /// </summary>
        /// <param name="obj">指定的<see cref="object"/>对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            var level = obj as LogLevel;
            return this == level;
        }
        /// <summary>
        /// 级别：调试，序号：0
        /// </summary>
        public static readonly LogLevel Debug = new LogLevel(0, "debug", "调试");
        /// <summary>
        /// 级别：信息，序号：1
        /// </summary>
        public static readonly LogLevel Info = new LogLevel(1, "info", "信息");
        /// <summary>
        /// 级别：警告，序号：2
        /// </summary>
        public static readonly LogLevel Warn = new LogLevel(2, "warn", "警告");
        /// <summary>
        /// 级别：错误，序号：3
        /// </summary>
        public static readonly LogLevel Error = new LogLevel(3, "error", "异常");
        /// <summary>
        /// 级别：致命，序号：4
        /// </summary>
        public static readonly LogLevel Fatal = new LogLevel(4, "fatal", "致命");
        /// <summary>
        /// 关闭日志记录
        /// </summary>
        public static readonly LogLevel Off = new LogLevel(5, "off", "关闭");
        internal static readonly LogLevel Min = Debug;
        internal static readonly LogLevel Max = Fatal;
        /// <summary>
        /// 比较两个<see cref="LogLevel"/>对象是否相等
        /// </summary>
        /// <param name="a">要比较的<see cref="LogLevel"/></param>
        /// <param name="b">要比较的<see cref="LogLevel"/></param>
        /// <returns>是否相等</returns>
        public static bool operator ==(LogLevel a, LogLevel b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return false;
            return a.order == b.order;
        }
        /// <summary>
        /// 比较两个<see cref="LogLevel"/>对象是否不相等
        /// </summary>
        /// <param name="a">要比较的<see cref="LogLevel"/></param>
        /// <param name="b">要比较的<see cref="LogLevel"/></param>
        /// <returns>是否不相等</returns>
        public static bool operator !=(LogLevel a, LogLevel b)
        {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return true;
            return a.order != b.order;
        }
        /// <summary>
        /// 比较第一个<see cref="LogLevel"/>对象是否小于第二个<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="a">第一个<see cref="LogLevel"/>对象</param>
        /// <param name="b">第二个<see cref="LogLevel"/>对象</param>
        /// <returns>是否小于</returns>
        public static bool operator <(LogLevel a, LogLevel b)
        {
            ParamGuard.NotNull(a, "a");
            ParamGuard.NotNull(b, "b");
            return a.order < b.order;
        }
        /// <summary>
        /// 比较第一个<see cref="LogLevel"/>对象是否小于或等于第二个<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="a">第一个<see cref="LogLevel"/>对象</param>
        /// <param name="b">第二个<see cref="LogLevel"/>对象</param>
        /// <returns>是否小于或等于</returns>
        public static bool operator <=(LogLevel a, LogLevel b)
        {
            ParamGuard.NotNull(a, "a");
            ParamGuard.NotNull(b, "b");
            return a.order <= b.order;
        }
        /// <summary>
        /// 比较第一个<see cref="LogLevel"/>对象是否大于第二个<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="a">第一个<see cref="LogLevel"/>对象</param>
        /// <param name="b">第二个<see cref="LogLevel"/>对象</param>
        /// <returns>是否大于</returns>
        public static bool operator >(LogLevel a, LogLevel b)
        {
            ParamGuard.NotNull(a, "a");
            ParamGuard.NotNull(b, "b");
            return a.order > b.order;
        }
        /// <summary>
        /// 比较第一个<see cref="LogLevel"/>对象是否大于或等于第二个<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="a">第一个<see cref="LogLevel"/>对象</param>
        /// <param name="b">第二个<see cref="LogLevel"/>对象</param>
        /// <returns>是否大于或等于</returns>
        public static bool operator >=(LogLevel a, LogLevel b)
        {
            ParamGuard.NotNull(a, "a");
            ParamGuard.NotNull(b, "b");
            return a.order >= b.order;
        }
        /// <summary>
        /// 将<see cref="LogLevel"/>对象转换为<see cref="int"/>对象
        /// </summary>
        /// <param name="level">要转换的<see cref="LogLevel"/>对象</param>
        /// <returns>转换得到的<see cref="int"/>对象</returns>
        public static implicit operator int(LogLevel level)
        {
            ParamGuard.NotNull(level, "level");
            return level.order;
        }
        /// <summary>
        /// 将<see cref="LogLevel"/>对象转换为<see cref="string"/>对象
        /// </summary>
        /// <param name="level">要转换的<see cref="LogLevel"/>对象</param>
        /// <returns>转换得到的<see cref="string"/>对象</returns>
        public static implicit operator string(LogLevel level)
        {
            ParamGuard.NotNull(level, "level");
            return level.name;
        }
        /// <summary>
        /// 将<see cref="int"/>对象转换为<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="level">要转换的<see cref="int"/>对象</param>
        /// <returns>转换得到的<see cref="LogLevel"/>对象</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 如果<param name="level">值的范围超过0~5
        /// </exception>
        public static implicit operator LogLevel(int level)
        {
            ParamGuard.InRange(level, 0, 5, "level");
            switch (level)
            {
                case 0:
                    return Debug;
                case 1:
                    return Info;
                case 2:
                    return Warn;
                case 3:
                    return Error;
                case 4:
                    return Fatal;
                case 5:
                    return Off;
                default:
                    throw new ArgumentException("未识别的日志等级:" + level);
            }
        }
        /// <summary>
        /// 将<see cref="string"/>对象转换为<see cref="LogLevel"/>对象
        /// </summary>
        /// <param name="level">要转换的<see cref="string"/>对象</param>
        /// <returns>转换得到的<see cref="LogLevel"/>对象</returns>
        /// <exception cref="ArgumentException">
        /// 如果<param name="level">不为以下值：debug、info、warn、error、fatal、off
        /// </exception>
        public static implicit operator LogLevel(string level)
        {
            ParamGuard.NotNull(level, "levelName");
            switch (level.ToLowerInvariant())
            {
                case "debug":
                    return Debug;
                case "info":
                    return Info;
                case "warn":
                    return Warn;
                case "error":
                    return Error;
                case "fatal":
                    return Fatal;
                case "off":
                    return Off;
                default:
                    throw new ArgumentException("未识别的日志类型:" + level);
            }
        }
    }
}
