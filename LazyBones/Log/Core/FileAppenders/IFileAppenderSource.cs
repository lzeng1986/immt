using System.IO;

namespace LazyBones.Log.Core.FileAppenders
{
    interface IFileAppenderSource
    {
        string AppendedFileName { get; }

        int ConcurrentWriteAttempts { get; }

        bool EnableConcurrentAccess { get; }

        bool CreateDirs { get; }

        bool EnableFileDelete { get; }

        int BufferSize { get; }

        FileAttributes FileAttributes { get; }
    }
}
