using System;
using System.Configuration;
using System.IO;
using System.Text;
using LazyBones.Config;

namespace LazyBones.Log
{
    static class TinyLog
    {
        static object lockObj = new object();
        /// <summary>
        /// 记录日志最小级别，低于此级别的日志将被忽略，可从配置文件appSettings添加tinyLog.MinLogLevel项进行配置
        /// </summary>
        /// <remarks>可设置的值：debug,warn,info,error,fatal,off</remarks>
        public static LogLevel MinLogLevel { get; internal set; }
        /// <summary>
        /// 是否将日志输出至控制台，可从配置文件appSettings添加tinyLog.LogToConsole项进行配置
        /// </summary>
        public static bool LogToConsole { get; internal set; }
        /// <summary>
        /// 是否将日志输出至标准错误输出流(Console.Error)，可从配置文件appSettings添加tinyLog.LogToConsoleError项进行配置
        /// </summary>
        public static bool LogToConsoleError { internal get; set; }
        /// <summary>
        /// 日志输出文件名称，可从配置文件appSettings添加tinyLog.LogFile项进行配置；
        /// 如果文件是相对路径，则文件创建在应用程序的根目录下
        /// </summary>
        /// <remarks>
        /// 应用程序在指定文件夹必须拥有写入权限
        /// tinyLog.LogFile项为空则表示关闭该功能；
        /// 设置路径时，需要提前创建路径上的文件夹，但文件不需创建，记录器不会抛出<see cref="DirectoryNotFoundException"/>异常</remarks>
        public static string LogFile { get; internal set; }
        /// <summary>
        /// 获取或设置日志输出流
        /// </summary>
        public static TextWriter LogWriter { get; set; }
        static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static TinyLog()
        {
            MinLogLevel = LoadAppSetting("tinyLog.MinLogLevel", LogLevel.Debug);
            LogToConsole = LoadAppSetting("tinyLog.LogToConsole", true);
            LogToConsoleError = LoadAppSetting("tinyLog.LogToConsoleError", false);
            LogFile = LoadAppSetting("tinyLog.LogFile", (string)null);
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Debug"/>级别信息
        /// </summary>
        public static bool IsDebugEnabled
        {
            get { return MinLogLevel <= LogLevel.Debug; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Info"/>级别信息
        /// </summary>
        public static bool IsInfoEnabled
        {
            get { return MinLogLevel <= LogLevel.Info; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Warn"/>级别信息
        /// </summary>
        public static bool IsWarnEnabled
        {
            get { return MinLogLevel <= LogLevel.Warn; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Error"/>级别信息
        /// </summary>
        public static bool IsErrorEnabled
        {
            get { return MinLogLevel <= LogLevel.Error; }
        }
        /// <summary>
        /// 获取记录器是否记录<see cref="LogLevel.Fatal"/>级别信息
        /// </summary>
        public static bool IsFatalEnabled
        {
            get { return MinLogLevel <= LogLevel.Fatal; }
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Debug"/>级别信息
        /// </summary>
        /// <param name="message">记录信息</param>
        public static void Debug(string message)
        {
            Write(LogLevel.Debug, message);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Debug"/>级别信息
        /// </summary>
        /// <param name="format">记录信息格式字符串</param>
        /// <param name="args">格式化参数</param>
        public static void Debug(string format, params object[] args)
        {
            Write(LogLevel.Debug, format, args);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Info"/>级别信息
        /// </summary>
        /// <param name="message">记录信息</param>
        public static void Info(string message)
        {
            Write(LogLevel.Info, message);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Info"/>级别信息
        /// </summary>
        /// <param name="format">记录信息格式字符串</param>
        /// <param name="args">格式化参数</param>
        public static void Info(string format, params object[] args)
        {
            Write(LogLevel.Info, format, args);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Warn"/>级别信息
        /// </summary>
        /// <param name="message">记录信息</param>
        public static void Warn(string message)
        {
            Write(LogLevel.Warn, message);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Warn"/>级别信息
        /// </summary>
        /// <param name="format">记录信息格式字符串</param>
        /// <param name="args">格式化参数</param>
        public static void Warn(string format, params object[] args)
        {
            Write(LogLevel.Warn, format, args);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Error"/>级别信息
        /// </summary>
        /// <param name="message">记录信息</param>
        public static void Error(string message)
        {
            Write(LogLevel.Error, message);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Error"/>级别信息
        /// </summary>
        /// <param name="format">记录信息格式字符串</param>
        /// <param name="args">格式化参数</param>
        public static void Error(string format, params object[] args)
        {
            Write(LogLevel.Error, format, args);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Fatal"/>级别信息
        /// </summary>
        /// <param name="message">记录信息</param>
        public static void Fatal(string message)
        {
            Write(LogLevel.Fatal, message);
        }
        /// <summary>
        /// 记录<see cref="LogLevel.Fatal"/>级别信息
        /// </summary>
        /// <param name="format">记录信息格式字符串</param>
        /// <param name="args">格式化参数</param>
        public static void Fatal(string format, params object[] args)
        {
            Write(LogLevel.Fatal, format, args);
        }
        static void Write(LogLevel level, string message, params object[] args)
        {
            if (level < MinLogLevel)
            {
                return;
            }

            //没有记录目标，则直接返回
            if (string.IsNullOrEmpty(LogFile) && !LogToConsole && !LogToConsoleError && LogWriter == null)
            {
                return;
            }

            try
            {
                var record = (args == null) ? message : string.Format(message, args);

                var builder = new StringBuilder(message.Length + 40);
                builder.Append("[");
                builder.Append(SystemClock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                builder.Append("] ");

                builder.Append(level.ToString());
                builder.Append(" ");
                builder.Append(record);
                var msg = builder.ToString();

                // Console支持多线程写入 [11/14/2013 zliang]
                if (LogToConsole)
                {
                    Console.WriteLine(msg);
                }
                if (LogToConsoleError)
                {
                    Console.Error.WriteLine(msg);
                }

                var logFile = LogFile;
                if (!string.IsNullOrEmpty(logFile))
                {
                    logFile = LazyBones.Config.AppDomainWrapper.GetFullPath(logFile);
                    using (var textWriter = File.AppendText(logFile))
                    {
                        textWriter.WriteLine(msg);
                    }
                }
                if (LogWriter != null)
                {
                    lock (lockObj)
                    {
                        LogWriter.WriteLine(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Check();
            }
        }

        static T LoadAppSetting<T>(string key, T defaultValue)  // 从配置文件的appSettings中加载配置 [11/14/2013 zliang]
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            try
            {
                object objValue;
                if (ConfigConvert.TryConvertFromString(typeof(T), value, out objValue))
                    return (T)objValue;
                else
                    return defaultValue;
            }
            catch (System.Exception ex)
            {
                ex.Check();
                return defaultValue;
            }
        }
    }
}
