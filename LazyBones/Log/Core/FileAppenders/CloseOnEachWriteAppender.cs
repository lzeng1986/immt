using System;
using System.IO;

namespace LazyBones.Log.Core.FileAppenders
{
    //每次写入时都会打开文件，并在写入完成后关闭文件
    class CloseOnEachWriteAppender : FileAppenderBase
    {
        public CloseOnEachWriteAppender(IAppendFileCreateParams param, string fileName)
            : base(param, fileName)
        {
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
        }

        public override bool TryGetFileInfo(out DateTime lastWriteTime, out long fileLength)
        {
            return Helper.GetFileInfo(FileName, out lastWriteTime, out fileLength);
        }

        public override void Write(byte[] bytes)
        {
            using (var fileStream = GetFileStream())
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
            Record();
        }
    }
}
