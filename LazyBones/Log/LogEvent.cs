using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using LazyBones.Log.Core;
using LazyBones.Log.Layouts;

namespace LazyBones.Log
{
    /// <summary>
    /// 表示日志记录对象
    /// </summary>
    public class LogEvent : IDisposable
    {
        static int globalLogEventId = 0;

        string formattedMessage = null;

        public LogEvent(LogLevel level, Logger logger, string message)
            : this(level, logger, message, null)
        {
        }
        public LogEvent(LogLevel level, Logger logger, string format, params object[] parameters)
        {
            this.TimeStamp = SystemClock.Now;
            this.Level = level;
            this.Logger = logger;
            this.LoggerName = logger == null ? string.Empty : logger.Name;
            formattedMessage = FormatMessage(format, parameters);
            this.LogEventId = Interlocked.Increment(ref globalLogEventId);
            this.LogThread = Thread.CurrentThread;
        }
        string FormatMessage(string format, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return format;

            try
            {
                return string.Format(format, parameters);
            }
            catch (System.Exception ex)
            {
                TinyLog.Error("构建LogEvent消息时出现错误：" + ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// 获取日志对象创建的时间
        /// </summary>
        public DateTime TimeStamp { get; private set; }
        /// <summary>
        /// 获取日志对象的日志等级
        /// </summary>
        public LogLevel Level { get; private set; }
        /// <summary>
        /// 获取日志对象的Id，此Id在应用程序范围内唯一
        /// </summary>
        public int LogEventId { get; private set; }
        /// <summary>
        /// 获取创建此日志对象的记录器
        /// </summary>
        public Logger Logger { get; private set; }
        /// <summary>
        /// 获取记创建此日志对象记录器的名称
        /// </summary>
        public string LoggerName { get; private set; }
        /// <summary>
        /// 获取或设置日志对象包含的错误
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// 日志对象中包含的记录信息
        /// </summary>
        public string Message
        {
            get { return formattedMessage; }
        }
        public StackTrace StackTrace { get; internal set; }
        public int StackFrameJumpCount { get; internal set; }
        public Thread LogThread { get; private set; }
        /// <summary>
        /// 获取或设置日志对象的扩展对象
        /// </summary>
        public object ExtensionObject { get; set; }
        Dictionary<Layout, string> cachedLayoutValue = null;
        object lockObj = new object();
        internal void AddCachedLayoutValue(Layout layout, string value)
        {
            lock (lockObj)
            {
                if (cachedLayoutValue == null)
                    cachedLayoutValue = new Dictionary<Layout, string>();
                cachedLayoutValue[layout] = value;
            }
        }
        internal bool TryGetCachedLayoutValue(Layout layout, out string value)
        {
            lock (lockObj)
            {
                value = null;
                if (cachedLayoutValue == null)
                {
                    return false;
                }
                return cachedLayoutValue.TryGetValue(layout, out value);
            }
        }

        public void Dispose()
        {
            if (cachedLayoutValue != null)
            {
                cachedLayoutValue.Clear();
                cachedLayoutValue = null;
            }
        }

        public readonly static LogEvent Empty = new LogEvent(LogLevel.Off, null, string.Empty);
    }
}
