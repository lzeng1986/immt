using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using LazyBones.Config;
using LazyBones.Extensions;
using LazyBones.Log.Config;

namespace LazyBones.Log
{
    //用于创建Logger的工厂
    internal class LogFactory : IDisposable
    {
        MultiFileSystemWatcher watcher = new MultiFileSystemWatcher();
        const int WatchDelay = 2000;
        Dictionary<LoggerKey, WeakReference> cachedLoggers = new Dictionary<LoggerKey, WeakReference>();
        Timer configFileReloadTimer;
        public event EventHandler LogConfigChanged;
        public LogFactory()
        {
            LoadConfig();
            watcher.FileChanged += FileChanged;
            configFileReloadTimer = new Timer(ReloadConfig, null, Timeout.Infinite, 0);//初始状态为关闭
        }
        void FileChanged(object sender, FileSystemEventArgs e)
        {
            //因为FileSystemWatcher可能会在一次操作中产生多个事件，比如：替换文件会产生delete和create事件
            //此句是过滤这些多余信息，防止多次加载
            //只有在WatchDelay时间内没有修改事件发生，才会重新加载配置
            //在修改文件后WatchDelay内再次修改文件，定时器将重新计算
            configFileReloadTimer.Change(WatchDelay, 0);
        }
        void ReloadConfig(object obj)
        {
            lock (this)
            {
                TinyLog.Info("日志配置文件变更，自动重新加载");
                LoadConfig();
                cachedLoggers.Clear();
                var handle = LogConfigChanged;
                if (handle != null)
                    handle(this, EventArgs.Empty);
            }
        }
        internal Logger GetLogger(LoggerKey key)
        {
            lock (this)
            {
                WeakReference wr = null;
                if (cachedLoggers.TryGetValue(key, out wr))
                {
                    if (wr.IsAlive && wr.Target is Logger)
                    {
                        return wr.Target as Logger;
                    }
                }
                Logger logger = null;
                if (key.loggerType != null && key.loggerType != typeof(Logger))
                {
                    try
                    {
                        logger = (Logger)key.loggerType.CreateInstance();
                    }
                    catch (System.Exception ex)
                    {
                        ex.Check();
                        TinyLog.Error("创建类型{0}记录器失败，错误：{1}", key.loggerType, ex);
                        key = new LoggerKey(typeof(Logger), key.loggerName);
                        logger = new Logger();
                    }
                }
                else
                {
                    logger = new Logger();
                }
                logger.Name = key.loggerName;
                SetConfigForLogger(logger);
                cachedLoggers[key] = new WeakReference(logger);
                return logger;
            }
        }
        LogConfig config = null;
        public LogConfig Config
        {
            get
            {
                lock (this)
                {
                    if (config != null)
                        return config;
                    LoadConfig();
                    return config;
                }
            }
        }
        void LoadConfig()
        {
            watcher.Stop();
            if (config != null)
            {
                config.Dispose();
                config = null;
            }
            config = LogConfig.Load("log.config");
            if (config.AutoReload)
            {
                watcher.Watch(config.ConfigFiles);
            }
            ThrowException = config.ThrowException;
        }
        void SetConfigForLogger(Logger logger)
        {
            if (config == null)
                return;
            foreach (var rule in config.LoggerRules.TakeWhile(r => !r.IsFinal).Where(r => r.CheckLoggerName(logger.Name)))
            {
                for (var i = 0; i <= LogLevel.Max.Order; i++)
                {
                    if (rule.CanLog(i))
                    {
                        foreach (var target in rule.Targets)
                        {
                            var node = new TargetLink(target, null);
                            if (logger.targetNodes[i] != null)
                                node.Next = logger.targetNodes[i];
                            logger.targetNodes[i] = node;
                        }
                    }
                }
            }
            logger.FindStackTraceUsage();
        }
        public void Dispose()
        {
            cachedLoggers.Clear();
            if (configFileReloadTimer != null)
            {
                configFileReloadTimer.Dispose();
                configFileReloadTimer = null;
            }
            if (config != null)
            {
                config.Dispose();
                config = null;
            }
            GC.SuppressFinalize(this);
        }
        internal bool ThrowException { get; set; }
    }
    //用于查找Logger时的key，记录loggerType是为了创建Logger子类
    internal class LoggerKey
    {
        internal readonly Type loggerType;
        internal readonly string loggerName;
        readonly int hashCode;
        public LoggerKey(Type loggerType, string loggerName)
        {
            this.loggerType = loggerType;
            this.loggerName = loggerName;
            Debug.Assert(loggerType != null);
            Debug.Assert(loggerName != null);
            hashCode = loggerType.GetHashCode() ^ loggerName.GetHashCode();
        }
        public override int GetHashCode()
        {
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            var key = obj as LoggerKey;
            if (ReferenceEquals(key, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            return (loggerName.Equals(key.loggerName)) && (loggerType.Equals(key.loggerType));
        }
        public static LoggerKey New(Type loggerType, string loggerName)
        {
            return new LoggerKey(loggerType, loggerName);
        }
    }
}
