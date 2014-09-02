using System;
using System.IO;
using System.Threading;
using LazyBones.Utils;

namespace LazyBones.Log.Core.FileAppenders
{
    abstract class FileAppenderBase : IDisposable
    {
        protected FileAppenderBase(IAppendFileCreateParams param, string fileName)
        {
            ParamGuard.NotNull(param, "param");
            CreateParams = param;
            CreateTime = SystemClock.Now;
            FileName = fileName;
        }
        public IAppendFileCreateParams CreateParams { get; private set; }
        public DateTime LastWriteTime { get; private set; }
        public DateTime CreateTime { get; private set; }
        public string FileName { get; private set; }

        public abstract void Flush();
        public abstract void Close();
        public abstract bool TryGetFileInfo(out DateTime lastWriteTime, out long fileLength);
        public abstract void Write(byte[] bytes);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }
        protected void Record()
        {
            this.LastWriteTime = SystemClock.Now;
        }
        protected void Record(DateTime dateTime)
        {
            this.LastWriteTime = dateTime;
        }

        Random random = new Random();
        protected FileStream GetFileStream()
        {
            for (var i = 0; i < CreateParams.ConcurrentWriteAttempts; i++) //尝试ConcurrentWriteAttempts次
            {
                try
                {
                    return CreateFileStream();
                }
                catch (DirectoryNotFoundException)
                {
                    if (!CreateParams.CreateDirs)
                        throw;
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                }
                catch (IOException)
                {
                    var delay = random.Next(1, 10) * 1000;  //随机等待1~10秒
                    TinyLog.Warn("打开文件[{0}]，第{1}次尝试失败，线程挂起{2}毫秒后再次尝试。", FileName, i, delay);
                    Thread.Sleep(delay);
                }
            }
            throw new InvalidOperationException("打开文件[" + FileName + "]失败");
        }
        FileStream CreateFileStream()
        {
            var fileShare = FileShare.Read;

            if (CreateParams.EnableConcurrentAccess)
            {
                fileShare |= FileShare.Write;
            }

            if (CreateParams.EnableFileDelete)
            {
                fileShare |= FileShare.Delete;
            }
            return new FileStream(FileName, FileMode.Append, FileAccess.Write, fileShare, CreateParams.BufferSize);
        }
    }
}
