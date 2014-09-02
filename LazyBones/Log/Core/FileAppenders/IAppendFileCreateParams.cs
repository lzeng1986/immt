using System.IO;

namespace LazyBones.Log.Core.FileAppenders
{
    public interface IAppendFileCreateParams
    {
        int ConcurrentWriteAttempts { get; }
        bool EnableConcurrentAccess { get; }
        bool CreateDirs { get; }
        bool EnableFileDelete { get; }
        int BufferSize { get; }
        FileAttributes FileAttributes { get; }
    }
}
