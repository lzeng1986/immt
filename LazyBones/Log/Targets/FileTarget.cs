using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LazyBones.Config;
using LazyBones.Log.Config;
using LazyBones.Log.Core.FileAppenders;
using LazyBones.Log.Layouts;

namespace LazyBones.Log.Targets
{
    //写入文件，支持日志自动存档
    [Target("file")]
    public class FileTarget : TargetWithLayout, IAppendFileCreateParams
    {
        static Dictionary<NewLineMode, string> NewLineModeMap = new Dictionary<NewLineMode, string>
        {
            {NewLineMode.None,string.Empty},
            {NewLineMode.Default,Environment.NewLine},
            {NewLineMode.CR,"\r"},
            {NewLineMode.LF,"\n"},
            {NewLineMode.CRLF,"\r\n"}
        };

        //定义不同FileArchiveMode的比较器，用于比较特定FileArchiveMode下两个DataTime是否相等
        static Dictionary<FileArchiveMode, Func<DateTime, DateTime, bool>> FileArchiveComparer = new Dictionary<FileArchiveMode, Func<DateTime, DateTime, bool>>
        {
            {FileArchiveMode.None,(d1,d2)=>false},
            {FileArchiveMode.Year,(d1,d2)=>d1.Year == d2.Year},
            {FileArchiveMode.Month,(d1,d2)=>d1.Year == d2.Year && d1.Month == d2.Month},
            {FileArchiveMode.Week,(d1,d2)=>Math.Abs((d1 - d2).TotalDays) < 7},//两个时间刻度差值小于7天
            {FileArchiveMode.Day,(d1,d2)=>d1.Year == d2.Year && d1.DayOfYear == d2.DayOfYear},
            {FileArchiveMode.Hour,(d1,d2)=>d1.Year == d2.Year && d1.DayOfYear == d2.DayOfYear && d1.Hour == d2.Hour},
            {FileArchiveMode.Minute,(d1,d2)=>d1.Year == d2.Year && d1.DayOfYear == d2.DayOfYear && d1.Hour == d2.Hour && d1.Minute == d2.Minute}
        };

        string newLine;
        NewLineMode newLineMode;
        [DefaultValue(NewLineMode.Default)]
        public NewLineMode NewLineMode
        {
            get { return newLineMode; }
            set
            {
                newLineMode = value;
                newLine = NewLineModeMap[newLineMode];
            }
        }

        [DefaultValue(FileAttributes.Normal)]
        public FileAttributes FileAttributes { get; set; }

        [DefaultValue(FileArchiveMode.None)]
        public FileArchiveMode ArchiveMode { get; set; }

        [DefaultValue(-1)]
        public long ArchiveSize { get; set; }

        [DefaultValue(9)]
        [Min(0)]
        public int MaxArchiveFiles { get; set; }

        [DefaultValue("utf-8")]
        public Encoding Encoding { get; set; }

        [DefaultValue(false)]
        public bool IncludeLineNo { get; set; }

        [DefaultValue(true)]
        public bool EnableFileDelete { get; set; }

        [DefaultValue(false)]
        public bool KeepFileOpen { get; set; }

        [DefaultValue(10)]
        public int ConcurrentWriteAttempts { get; set; }

        [Required]
        public Layout FileName { get; set; }

        [DefaultValue(false)]
        public bool CreateNew { get; set; }

        [DefaultValue(true)]
        public bool CreateDirs { get; set; }

        [DefaultValue(true)]
        public bool AutoFlush { get; set; }

        [DefaultValue(false)]
        public bool EnableConcurrentAccess { get; set; }

        [DefaultValue(65536)]
        [Min(1024)]
        public int BufferSize { get; set; }

        [DefaultValue(5)]
        public int OpenFileCacheSize { get; set; }

        [DefaultValue(-1)]
        public int OpenFileCacheTimeout { get; set; }

        [DefaultValue(false)]
        public bool DeleteOldFileOnStartup { get; set; }

        public string AppendFileName { get; private set; }

        static char[] InvalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray();

        List<FileAppenderBase> recentFileAppenders;
        Timer autoClosingTimer;
        Func<string, FileAppenderBase> fileAppenderCreator;
        Dictionary<string, DateTime> initializedFiles = new Dictionary<string, DateTime>();

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            recentFileAppenders = new List<FileAppenderBase>(OpenFileCacheSize);
            if ((this.OpenFileCacheSize > 0 || this.EnableFileDelete) && this.OpenFileCacheTimeout > 0)
            {
                this.autoClosingTimer = new Timer(AutoClosingTimerCallback, null,
                    this.OpenFileCacheTimeout * 1000,
                    Timeout.Infinite);
            }
            if (!this.KeepFileOpen)
            {
                fileAppenderCreator = (name) => new CloseOnEachWriteAppender(this, name);
            }
            else
            {
                if (this.EnableConcurrentAccess)
                {
                    fileAppenderCreator = (name) => new SyncFileAppender(this, name);
                }
                else
                {
                    fileAppenderCreator = (name) => new KeepOpenFileAppender(this, name);
                }
            }
        }
        protected override void CloseTarget()
        {
            base.CloseTarget();

            foreach (string fileName in this.initializedFiles.Keys.ToList())
            {
                this.WriteFooterAndUninitialize(fileName);
            }

            if (this.autoClosingTimer != null)
            {
                this.autoClosingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.autoClosingTimer.Dispose();
                this.autoClosingTimer = null;
            }
            recentFileAppenders.ForEach(a => a.Close());
            recentFileAppenders.Clear();
            recentFileAppenders = null;
        }
        void AutoClosingTimerCallback(object state)//自动存档定时器回调
        {
            lock (this.SyncRoot)
            {
                if (!this.Initialized)
                {
                    return;
                }
                //关闭CreateTime小于timeToKill的FileAppender，并从recentFileAppenders中移除
                try
                {
                    DateTime timeToKill = SystemClock.Now.AddSeconds(-this.OpenFileCacheTimeout);
                    for (var i = 0; i < recentFileAppenders.Count; i++)
                    {
                        var appender = recentFileAppenders[i];
                        if (appender == null)
                            break;
                        if (appender.CreateTime < timeToKill)
                        {
                            appender.Close();
                            recentFileAppenders[i] = null;
                        }
                    }
                    //剔除recentFileAppenders中的null
                    recentFileAppenders.RemoveAll(a => a == null);
                }
                catch (Exception exception)
                {
                    exception.Check();
                    TinyLog.Warn("自动存档回调出现错误: {0}", exception);
                }
                autoClosingTimer.Change(this.OpenFileCacheTimeout * 1000, Timeout.Infinite);
            }
        }

        protected override void Write(LogEvent logEvent)
        {
            var fileName = FileName.GetFormatMessage(logEvent);
            var fullPath = AppDomainWrapper.GetFullPath(fileName);

            var dir = Path.GetDirectoryName(fullPath);
            foreach (var c in Path.GetInvalidPathChars())
                dir = dir.Replace(c, '_');

            var name = fullPath.Substring(dir.Length + 1);
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            fileName = Path.Combine(dir, name);
            byte[] bytes = this.GetBytesToWrite(logEvent);

            if (this.NeedArchive(fileName, logEvent, bytes.Length))
            {
                InvalidateCacheItem(fileName);
                ArchiveFile(fileName);
            }

            this.WriteToFile(fileName, bytes, true);
        }
        static string ValidateFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName;
        }
        void InvalidateCacheItem(string fileName)
        {
            var ind = recentFileAppenders.FindIndex(a => a.FileName == fileName);
            if (ind != -1)
            {
                recentFileAppenders[ind].Close();
                recentFileAppenders.RemoveAt(ind);
            }
        }

        public void CleanupInitializedFiles()//关闭所有两天前初始化的文件
        {
            this.CleanupInitializedFiles(SystemClock.Now.AddDays(-2));
        }

        public void CleanupInitializedFiles(DateTime cleanupThreshold)
        {
            var filesToUninitialize = this.initializedFiles.Where(o => o.Value < cleanupThreshold).Select(o => o.Key).ToList();

            foreach (string fileName in filesToUninitialize)
            {
                this.WriteFooterAndUninitialize(fileName);
            }
        }
        void WriteFooterAndUninitialize(string fileName)
        {
            var footerBytes = this.GetFooterBytes();
            if (footerBytes != null && File.Exists(fileName))
            {
                this.WriteToFile(fileName, footerBytes, false);
            }
            this.initializedFiles.Remove(fileName);//!!!这里会报错(在程序集卸载，从CloseTarget调用时)，fileName为null，原因不明，key值不应为null才是！！！
        }

        byte[] GetBytesToWrite(LogEvent logEvent)
        {
            var renderedText = this.Body.GetFormatMessage(logEvent) + this.newLine;
            return OnBeforeWrite(Encoding.GetBytes(renderedText));
        }
        byte[] GetHeaderBytes()
        {
            if (Header == null)
                return null;
            var text = Header.GetFormatMessage(LogEvent.Empty) + this.newLine;
            return OnBeforeWrite(Encoding.GetBytes(text));
        }
        byte[] GetFooterBytes()
        {
            if (Footer == null)
                return null;
            var text = Footer.GetFormatMessage(LogEvent.Empty) + this.newLine;
            return OnBeforeWrite(Encoding.GetBytes(text));
        }
        protected virtual byte[] OnBeforeWrite(byte[] bytes)
        {
            return bytes;
        }

        bool NeedArchive(string fileName, LogEvent logEvent, int writeSize)//判断是否需要存档
        {
            if (ArchiveSize == -1 || ArchiveMode == FileArchiveMode.None)
                return false;

            DateTime lastWriteTime;
            long fileLength;

            if (TryGetFileInfo(fileName, out lastWriteTime, out fileLength))
            {
                if (fileLength + writeSize > ArchiveSize)
                    return true;

                return FileArchiveComparer[ArchiveMode](lastWriteTime, logEvent.TimeStamp);
            }
            return false;
        }

        static Regex numberPattern = new Regex("{#+}", RegexOptions.Compiled);
        void ArchiveFile(string fileName)   //实现自动存档功能，编号从1开始，自动增长
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            var dir = Path.GetDirectoryName(Path.GetFullPath(fileName));

            var match = numberPattern.Match(fileName);
            var numDigits = 0;
            string fileNamePattern;
            if (match.Success)
            {
                numDigits = match.Value.Length - 2;
                fileNamePattern = fileName;
            }
            else
            {
                for (var num = MaxArchiveFiles; num > 0; numDigits++, num /= 10) ;
                fileNamePattern = fileName + '{' + new string('#', numDigits) + '}';
            }

            var fileNameMask = numberPattern.Replace(fileName, "*");

            var number2name = new SortedList<int, string>();
            if (Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            foreach (var file in Directory.GetFiles(dir, fileNameMask))
            {
                var numStr = numberPattern.Match(file).Value;
                numStr = numStr.Substring(1, numStr.Length - 2);
                int num;
                if (int.TryParse(numStr, out num))
                {
                    number2name.Add(num, file);
                }
            }

            while (number2name.Count >= MaxArchiveFiles)    //warning：这里必须确保MaxArchiveFiles>0，否则会抛出异常
            {
                File.Delete(number2name.First().Value);
                number2name.RemoveAt(0);
            }

            var nextNum = number2name.Keys.LastOrDefault() + 1;
            var newFileName = numberPattern.Replace(fileNamePattern, nextNum.ToString().PadLeft(numDigits));
            File.Move(fileName, newFileName);
        }


        bool TryGetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            var appender = recentFileAppenders.Find(a => a.FileName == fileName);
            if (appender != null)
            {
                return appender.TryGetFileInfo(out lastWriteTime, out fileLength);
            }
            else
            {
                fileLength = -1;
                lastWriteTime = DateTime.MinValue;
                return false;
            }
        }

        void WriteToFile(string fileName, byte[] writeData, bool withHeader)
        {
            if (CreateNew)//CreateNew为true，则每次写入都将创建一个文件或者清空同名文件
            {
                using (var fs = File.Create(fileName))
                {
                    var headerBytes = GetHeaderBytes();
                    var footerBytes = GetFooterBytes();
                    if (headerBytes != null)
                        fs.Write(headerBytes, 0, headerBytes.Length);
                    fs.Write(writeData, 0, writeData.Length);
                    if (footerBytes != null)
                        fs.Write(footerBytes, 0, footerBytes.Length);
                }
            }
            else
            {
                var ind = recentFileAppenders.FindIndex(a => a.FileName == fileName);
                var appenderToWrite = ind == -1 ? fileAppenderCreator(fileName) : recentFileAppenders[ind];
                //将最近使用的FileAppender放在第一位，以提高性能
                if (ind != -1)
                {
                    recentFileAppenders.RemoveAt(ind);
                }
                else if (recentFileAppenders.Count == OpenFileCacheSize)
                {
                    recentFileAppenders[recentFileAppenders.Count - 1].Close();
                    recentFileAppenders.RemoveAt(recentFileAppenders.Count - 1);
                }
                recentFileAppenders.Insert(0, appenderToWrite);

                if (withHeader && NeedWriteHeader(fileName))
                {
                    long fileLength;
                    DateTime lastWriteTime;

                    //只在空文件或者获取文件信息失败时写入Header
                    if (!appenderToWrite.TryGetFileInfo(out lastWriteTime, out fileLength) || fileLength == 0)
                    {
                        var headerBytes = this.GetHeaderBytes();
                        if (headerBytes != null)
                        {
                            appenderToWrite.Write(headerBytes);
                        }
                    }
                }

                appenderToWrite.Write(writeData);
            }
        }

        bool NeedWriteHeader(string fileName)//判断是否需要写入Header
        {
            //initializedFiles中保存了已经初始化的文件名
            //如果initializedFiles中不包含fileName，则说明此fileName为第一次写入，需要添加头文件
            //initializedFiles缓冲区大小为100，如果超过限制，则强行关闭所有initializedFiles缓冲内容
            if (!this.initializedFiles.ContainsKey(fileName))
            {
                if (this.DeleteOldFileOnStartup)
                {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch (Exception ex)
                    {
                        ex.Check();
                        TinyLog.Warn("删除文件失败 '{0}': {1}", fileName, ex);
                    }
                }

                if (initializedFiles.Count > 100)
                {
                    CleanupInitializedFiles();
                }

                this.initializedFiles[fileName] = SystemClock.Now;
                return true;
            }
            else
            {
                this.initializedFiles[fileName] = SystemClock.Now;
                return false;
            }
        }
    }
}
