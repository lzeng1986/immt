using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LazyBones.Log.Core;
using LazyBones.Log.Filters;
using LazyBones.Log.Targets;

namespace LazyBones.Log
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 记录器的名称
        /// </summary>
        public string Name { get; internal set; }
        internal TargetLink[] targetNodes = new TargetLink[(int)LogLevel.Max - (int)LogLevel.Min + 1];
        internal bool[] useStackTrace = new bool[(int)LogLevel.Max - (int)LogLevel.Min + 1];
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Debug"/>级别信息
        /// </summary>
        public bool LogDebugEnabled
        {
            get { return targetNodes[LogLevel.Debug.Order] != null; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Info"/>级别信息
        /// </summary>
        public bool LogInfoEnabled
        {
            get { return targetNodes[LogLevel.Info.Order] != null; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Warn"/>级别信息
        /// </summary>
        public bool LogWarnEnabled
        {
            get { return targetNodes[LogLevel.Warn.Order] != null; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Error"/>级别信息
        /// </summary>
        public bool LogErrorEnabled
        {
            get { return targetNodes[LogLevel.Error.Order] != null; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Fatal"/>级别信息
        /// </summary>
        public bool LogFatalEnabled
        {
            get { return targetNodes[LogLevel.Fatal.Order] != null; }
        }
        /// <summary>
        /// 查询某一日志级别是否被该<see cref="Logger"/>对象记录
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns>是否被记录</returns>
        public bool CanLog(LogLevel logLevel)
        {
            return targetNodes[logLevel] != null;
        }
        public void Debug(string message)
        {
            Write(LogLevel.Debug, message);
        }
        public void Debug(string format, params object[] args)
        {
            Write(LogLevel.Debug, format, args);
        }
        public void Info(string message)
        {
            Write(LogLevel.Info, message);
        }
        public void Info(string format, params object[] args)
        {
            Write(LogLevel.Info, format, args);
        }
        public void Warn(string message)
        {
            Write(LogLevel.Warn, message);
        }
        public void Warn(string format, params object[] args)
        {
            Write(LogLevel.Warn, format, args);
        }
        public void Error(string message)
        {
            Write(LogLevel.Error, message);
        }
        public void Error(string format, params object[] args)
        {
            Write(LogLevel.Error, format, args);
        }
        public void Error(Exception exception, string message)
        {
            Write(LogLevel.Error, exception, message);
        }
        public void Error(Exception exception, string format, params object[] args)
        {
            Write(LogLevel.Error, exception, format, args);
        }
        public void Fatal(string message)
        {
            Write(LogLevel.Fatal, message);
        }
        public void Fatal(string format, params object[] args)
        {
            Write(LogLevel.Fatal, format, args);
        }
        public void Fatal(Exception exception, string message)
        {
            Write(LogLevel.Fatal, message, exception);
        }
        public void Fatal(Exception exception, string format, params object[] args)
        {
            Write(LogLevel.Fatal, exception, format, args);
        }
        public void Log(LogLevel level, string message)
        {
            Write(level, message);
        }
        public void Log(LogLevel level, string format, params object[] args)
        {
            Write(level, format, args);
        }
        public void Log(LogLevel level, Exception exception, string message)
        {
            Write(level, exception, message);
        }
        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
            Write(level, exception, format, args);
        }

        void Write(LogLevel level, string format, params object[] args)
        {
            var logEvent = new LogEvent(level, this, format, args);
            WriteInternal(level, logEvent);
        }
        void Write(LogLevel level, Exception exception, string message)
        {
            var logEvent = new LogEvent(level, this, message);
            logEvent.Exception = exception;
            WriteInternal(level, logEvent);
        }
        void Write(LogLevel level, Exception exception, string format, params object[] args)
        {
            var logEvent = new LogEvent(level, this, format, args);
            logEvent.Exception = exception;
            WriteInternal(level, logEvent);
        }
        void WriteInternal(LogLevel level, LogEvent logEvent)
        {
            if (useStackTrace[level.Order])
                FindAndSetStackTrace(logEvent, this.GetType());

            WriteToTarget(targetNodes[level.Order], logEvent);
        }
        internal void FindStackTraceUsage()//寻找是否在目标中需要添加StackTrace，因为添加StackTrace是一个耗时操作
        {
            for (var i = 0; i < targetNodes.Length; i++)
            {
                var node = targetNodes[i];
                while (node != null)
                {
                    useStackTrace[i] |= Helper.GetObjects<LazyBones.Log.Config.IUseStackTrace>(node.Target).Any();
                    node = node.Next;
                }
            }
        }

        static readonly Assembly[] IgnoredAssembly = new[] { typeof(Logger).Assembly, typeof(string).Assembly, typeof(Debug).Assembly };

        static void FindAndSetStackTrace(LogEvent logEvent, Type loggerType) //在调用堆栈上寻找调用Logger的方法，并将信息设置到LogEvent
        {
            var st = new StackTrace(0, true);
            int firstUserFrame = 0;
            for (int i = 0; i < st.FrameCount; ++i)
            {
                var frame = st.GetFrame(i);
                var mb = frame.GetMethod();
                Assembly methodAssembly = null;

                if (mb.DeclaringType != null)
                {
                    methodAssembly = mb.DeclaringType.Assembly;
                }

                if ((loggerType == null && IgnoredAssembly.Contains(methodAssembly)) || mb.DeclaringType == loggerType)
                {
                    firstUserFrame = i + 1;
                }
                else
                {
                    if (firstUserFrame != 0)
                    {
                        break;
                    }
                }
            }
            logEvent.StackTrace = st;
            logEvent.StackFrameJumpCount = firstUserFrame;
        }

        static void WriteToTarget(TargetLink targetLink, LogEvent logEvent)
        {
            while (targetLink != null)
            {
                targetLink.Target.WriteLogEvent(logEvent);
                targetLink = targetLink.Next;
            }
        }

    }
    /// <summary>
    /// <see cref="Target"/>对象的链表
    /// </summary>
    internal class TargetLink
    {
        public TargetLink(Target target, IEnumerable<Filter> filters)
        {
            Target = target;
            if (filters != null)
                Filters = filters.ToArray();
        }
        /// <summary>
        /// 该<see cref="TargetLink"/>对象对应的<see cref="Target"/>对象
        /// </summary>
        public readonly Target Target;
        /// <summary>
        /// 该<see cref="TargetLink"/>对象的下一个链表对象
        /// </summary>
        public TargetLink Next = null;
        /// <summary>
        /// 
        /// </summary>
        public readonly Filter[] Filters;
    }
}
