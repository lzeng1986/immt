using System;
using System.IO;

namespace LazyBones.Log.Core.FileAppenders
{
    //一直保持文件打开
    class KeepOpenFileAppender : FileAppenderBase
    {
        FileStream fileStream;
        long currentFileLength;

        public KeepOpenFileAppender(IAppendFileCreateParams param, string fileName)
            : base(param, fileName)
        {
            this.fileStream = GetFileStream();
            var fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                Record(fi.LastWriteTime);
                currentFileLength = fi.Length;
            }
            else
            {
                Record();
                currentFileLength = 0;
            }
        }
        public override void Flush()
        {
            if (fileStream == null)
                return;
            fileStream.Flush();
            Record();
        }

        public override void Close()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        public override bool TryGetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
            lastWriteTime = this.LastWriteTime;
            fileLength = this.currentFileLength;
            return true;
        }

        public override void Write(byte[] bytes)
        {
            if (fileStream != null)
            {
                fileStream.Write(bytes, 0, bytes.Length);
                currentFileLength += bytes.Length;
                Record();
            }
        }
    }
}
