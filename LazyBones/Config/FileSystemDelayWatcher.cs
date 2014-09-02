using System;
using System.IO;
using System.Threading;

namespace LazyBones.Config
{
    //具有延迟功能的FileSystemWatcher
    public class FileSystemDelayWatcher : IDisposable
    {
        public int Delay { get; set; }
        public event FileSystemEventHandler FileChanged;
        Timer delayTimer;

        FileSystemWatcher watcher = null;
        volatile NotifyFilters filters = NotifyFilters.LastWrite |
                                NotifyFilters.CreationTime |
                                NotifyFilters.Size |
                                NotifyFilters.Security |
                                NotifyFilters.Attributes;

        public NotifyFilters Filters
        {
            get { return filters; }
            set { filters = value; }
        }

        public FileSystemDelayWatcher()
        {
            Delay = 2000;
            delayTimer = new Timer(TimerCallback, null, Timeout.Infinite, 0);//初始状态为关闭
        }

        void TimerCallback(object obj)
        {
            var handler = FileChanged;
            if (handler != null)
                FileChanged(this, lastArgs);
        }

        public void Watch(string filePath)
        {
            filePath = AppDomainWrapper.GetFullPath(filePath);
            if (string.IsNullOrEmpty(filePath))
                return;
            Stop();
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = filters
            };

            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.EnableRaisingEvents = true;
        }
        FileSystemEventArgs lastArgs;
        void OnChanged(object sender, FileSystemEventArgs args)
        {
            lastArgs = args;
            delayTimer.Change(Delay, 0);
        }

        public void Stop()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }
        }

        public void Dispose()
        {
            Stop();
            if (delayTimer != null)
            {
                delayTimer.Dispose();
                delayTimer = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
