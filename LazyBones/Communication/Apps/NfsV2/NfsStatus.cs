
namespace LazyBones.Communication.Apps.NfsV2
{
    /// <summary>
    /// rfc1094[1989.3]-[2.3.1]-[page 12-14]
    /// nfs服务回复的状态值
    /// </summary>
    public enum NfsStatus
    {
        /// <summary>
        /// the call completed successfully and the results are valid
        /// The other values indicate some kind of error occurred on the server side during the servicing of the procedure.
        /// </summary>
        OK = 0,
        /// <summary>
        /// Not owner. The caller does not have correct ownership to perform the requested operation.
        /// </summary>
        NFSERR_PERM = 1,
        /// <summary>
        /// No such file or directory. The file or directory specified does not exist.
        /// </summary>
        NotExist = 2,
        /// <summary>
        /// Some sort of hard error occurred when the operation was in progress. This could be a disk error, for example.
        /// </summary>
        IO = 5,
        /// <summary>
        /// No such device or address.
        /// </summary>
        NoSuchDeviceOrAddress = 6,
        /// <summary>
        /// Permission denied. The caller does not have the correct permission to perform the requested operation.
        /// </summary>
        AccessDenied = 13,
        /// <summary>
        /// File exists. The file specified already exists.
        /// </summary>
        FileExist = 17,
        /// <summary>
        /// No such device.
        /// </summary>
        NoSuchDevice = 19,
        /// <summary>
        /// Not a directory. The caller specified a non-directory in a directory operation.
        /// </summary>
        NotDirectory = 20,
        /// <summary>
        /// Is a directory. The caller specified a directory in a nondirectory operation.
        /// </summary>
        IsDirectory = 21,
        /// <summary>
        /// File too large. The operation caused a file to grow beyond the server’s limit.
        /// </summary>
        FileTooBig = 27,
        /// <summary>
        /// No space left on device. The operation caused the server’s filesystem to reach its limit.
        /// </summary>
        NotEnoughSpace = 28,
        /// <summary>
        /// Read-only filesystem. Write attempted on a read-only filesystem.
        /// </summary>
        ReadOnlySystem = 30,
        /// <summary>
        /// File name too long. The file name in an operation was too long.
        /// </summary>
        NameTooLong = 63,
        /// <summary>
        /// Directory not empty. Attempted to remove a directory that was not empty.
        /// </summary>
        DirNotEmpty = 66,
        /// <summary>
        /// Disk quota exceeded. The client’s disk quota on the server has been exceeded.
        /// </summary>
        QuotaExceed = 69,
        /// <summary>
        /// The "fhandle" given in the arguments was invalid. That is, the file referred to by that file handle no longer exists, or access to it has been revoked.
        /// </summary>
        FHandleInvalid = 70,
        /// <summary>
        /// The server’s write cache used in the "WRITECACHE" call got flushed to disk.
        /// </summary>
        NFSERR_WFLUSG = 99
    }
}
