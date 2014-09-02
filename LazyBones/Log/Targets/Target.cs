using System;
using System.Collections.Generic;
using LazyBones.Log.Config;

namespace LazyBones.Log.Targets
{
    public abstract class Target : IDisposable, IInitializable
    {
        object syncObj = new object();
        protected object SyncRoot
        {
            get { return syncObj; }
        }
        bool initialized = false;
        public bool Initialized
        {
            get { return initialized; }
        }
        public string Name { get; set; }
        protected LogConfig Config { get; private set; }
        public void WriteLogEvent(LogEvent logEvent)
        {
            lock (syncObj)
            {
                if (!initialized)
                    return;
                try
                {
                    Write(logEvent);
                }
                catch (Exception ex)
                {
                    ex.Check();
                    TinyLog.Error("写入目标<{0}>时出现错误:{1}", Name, ex);
                }
            }
        }
        public void WriteLogEvents(IEnumerable<LogEvent> logEvents)
        {
            if (logEvents == null)
            {
                return;
            }
            lock (syncObj)
            {
                if (!initialized)
                    return;
                try
                {
                    foreach (var logEvent in logEvents)
                    {
                        Write(logEvent);
                    }
                }
                catch (Exception ex)
                {
                    ex.Check();
                    TinyLog.Error("写入Target<{0}>时出现错误:{1}", Name, ex);
                }
            }
        }
        protected virtual void InitializeTarget() { }
        protected virtual void CloseTarget() { }
        protected virtual void Write(LogEvent logEvent)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (syncObj)
                {
                    if (initialized)
                    {
                        initialized = false;
                        try
                        {
                            this.CloseTarget();
                        }
                        catch (Exception ex)
                        {
                            ex.Check();
                            TinyLog.Error("释放Target<{0}>时出现错误:{1}", Name, ex);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(LogConfig logConfig)
        {
            lock (syncObj)
            {
                if (initialized)
                    return;
                Config = logConfig;
                try
                {
                    InitializeTarget();
                    initialized = true;
                }
                catch (Exception ex)
                {
                    TinyLog.Error("初始化Target<{0}>时出现错误：{1}", this.GetType(), ex);
                }
            }
        }
        public override string ToString()
        {
            return string.Format("name:{0} type:{1}", Name, GetType());
        }
    }
}
