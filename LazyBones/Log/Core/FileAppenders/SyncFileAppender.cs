using System;
using System.IO;
using System.Threading;

namespace LazyBones.Log.Core.FileAppenders
{
    //进程同步
    class SyncFileAppender : FileAppenderBase
    {
        Mutex mutex;
        FileStream fileStream;
        public SyncFileAppender(IAppendFileCreateParams param,string fileName)
            : base(param, fileName)
        {
            try
            {
                mutex = CreateMutex();
            }
            catch
            {
                if (mutex != null)
                {
                    mutex.Close();
                    mutex = null;
                }
                throw;
            }
            fileStream = GetFileStream();
        }
        public override void Flush()
        {
        }

        public override void Close()
        {
            if (this.mutex != null)
            {
                this.mutex.Close();
            }

            if (this.fileStream != null)
            {
                this.fileStream.Close();
            }

            this.mutex = null;
            this.fileStream = null;
            Record();
        }

        public override bool TryGetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
            return Helper.GetFileInfo(FileName, out lastWriteTime, out fileLength);
        }

        public override void Write(byte[] bytes)
        {
            if (mutex == null)
            {
                return;
            }

            try
            {
                this.mutex.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // ignore the exception, another process was killed without properly releasing the mutex
                // the mutex has been acquired, so proceed to writing
                // See: http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
            }

            try
            {
                fileStream.Seek(0, SeekOrigin.End);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Flush();
                Record();
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

        Mutex CreateMutex()
        {
            for (var i = 0; i < 10; i++)
            {
                bool createdNew;
                var name = Guid.NewGuid().ToString();
                var mutex = new Mutex(false, name, out createdNew);
                if (createdNew)
                    return mutex;
            }
            throw new InvalidOperationException("创建Mutex失败");
        }
    }
}
