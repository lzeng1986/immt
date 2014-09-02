using System;
using System.Collections.Generic;
using LazyBones.Log.Config;

namespace LazyBones.Log.Targets
{
    public abstract class TargetBase : IDisposable, IInitializable
    {
        private object lockObj = new object();
        protected object SyncRoot
        {
            get { return lockObj; }
        }
        bool initialized = false;

        public string Name { get; set; }

        protected LogConfig Config { get; private set; }
        public void WriteLogEvent(LogEvent logEvent)
        {
            try
            {
                Write(logEvent);
            }
            catch (Exception ex)
            {
                TinyLog.Error("写入目标<{0}>时出现错误<1>：", this.GetType(), ex);
            }
        }
        public void WriteLogEvents(IEnumerable<LogEvent> logEvents)
        {
            if (logEvents == null)
            {
                return;
            }
            foreach (var logEvent in logEvents)
            {
                WriteLogEvent(logEvent);
            }
        }
        protected virtual void InitializeTarget() { }
        protected virtual void CloseTarget() { }
        protected abstract void Write(LogEvent logEvent);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CloseTarget();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(LogConfig logConfig)
        {
            lock (lockObj)
            {
                if (initialized)
                    return;
                Config = logConfig;
                initialized = true;
                try
                {
                    InitializeTarget();
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
