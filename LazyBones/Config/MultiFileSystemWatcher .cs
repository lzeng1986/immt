using System;
using System.Collections.Generic;
using System.IO;

namespace LazyBones.Config
{
    /// <summary>
    /// 用于同时监控多个文件
    /// </summary>
    public class MultiFileSystemWatcher : IDisposable
    {
        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        volatile NotifyFilters filters = NotifyFilters.LastWrite |
                                NotifyFilters.CreationTime |
                                NotifyFilters.Size |
                                NotifyFilters.Security |
                                NotifyFilters.Attributes;
        /// <summary>
        /// 获取或设置要监视的更改类型
        /// </summary>
        public NotifyFilters Filters
        {
            get { return filters; }
            set { filters = value; }
        }
        /// <summary>
        /// 监视多个文件
        /// </summary>
        /// <param name="filePaths">多个文件路径</param>
        public void Watch(params string[] filePaths)
        {
            if (filePaths == null)
                return;
            foreach (var path in filePaths)
            {
                Watch(path);
            }
        }
        /// <summary>
        /// 监视单个文件
        /// </summary>
        /// <param name="filePath">单个文件路径</param>
        public void Watch(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
            filePath = AppDomainWrapper.GetFullPath(filePath);
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

            lock (this)
            {
                this.watchers.Add(watcher);
            }
        }
        /// <summary>
        /// 当监控文件变化时发生，该事件是线程安全的
        /// </summary>
        public event FileSystemEventHandler FileChanged;
        void OnChanged(object sender, FileSystemEventArgs args)
        {
            var handler = FileChanged;
            if (handler != null)
                FileChanged(this, args);
        }
        /// <summary>
        /// 停止监控，并释放资源
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                foreach (var watcher in watchers)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                watchers.Clear();
            }
        }
        /// <summary>
        /// 停止监控，并释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
            //阻止调用多余的垃圾回收
            GC.SuppressFinalize(this);
        }
    }
}
