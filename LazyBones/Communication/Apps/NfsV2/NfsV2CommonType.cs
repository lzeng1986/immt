using System;
using System.IO;
using LazyBones.Extensions;

namespace LazyBones.Communication.Apps.NfsV2
{
    public class FileHandle
    {
        public readonly int Id;
        public FileHandle(int id)
        {
            Id = id;
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public void AppendToRpcPacket(RpcPacket packet)
        {
            packet.Append(Id);
            packet.Append(0);
            packet.Append(0);
            packet.Append(0);
            packet.Append(0);
            packet.Append(0);
            packet.Append(0);
            packet.Append(0);
        }
        public static FileHandle FromRpcPacket(RpcPacket packet)
        {
            var id = packet.GetInt32();
            packet.Jump(32 - 4);
            return new FileHandle(id);
        }
    }
    
    /// <summary>
    /// rfc1094[1989.3]-[2.3.2]-[page 15]
    /// rfc1094中的 "ftype" ，用于表示文件的类型
    /// </summary>
    public enum NfsFileType
    {
        NFNON = 0,
        NFREG = 1,
        NFDIR = 2,
        NFBLK = 3,
        NFCHR = 4,
        NFLNK = 5
    };
    /// <summary>
    /// rfc1094[1989.3]-[2.3.5]-[page 16-17]
    /// 用于设置文件的类型及访问权限，协议中是8进制，这里转化为10进制
    /// </summary>
    [Flags]
    public enum NfsFileMode
    {
        DIR = 16384,
        CHR = 8192,
        BLK = 24576,
        FILE = 32768,
        LNK = 40960,
        NON = 49152,
        SUID = 2048,
        SGID = 1024,
        SWAP = 512,
        OWNREAD = 256,
        OWNWRITE = 128,
        OWNEXEC = 64,
        GROUPREAD = 32,
        GROUPWRITE = 16,
        GROUPEXEC = 8,
        WORLDREAD = 4,
        WORLDWRITE = 2,
        WORLDEXEC = 1
    }
    //2.3.10. diropargs
    //struct diropargs {
    //fhandle dir;
    //filename name;
    //};
    //The "diropargs" structure is used in directory operations. The
    //"fhandle" "dir" is the directory in which to find the file "name".
    //A directory operation is one in which the directory is affected.
    public class DirAgrs
    {
        public FileHandle DirHandle { get; private set; }
        public string Name { get; private set; }
        private DirAgrs() { }
        public static DirAgrs FromRpcPacket(RpcPacket packet)
        {
            var args = new DirAgrs();
            args.DirHandle = FileHandle.FromRpcPacket(packet);
            args.Name = packet.GetString();
            return args;
        }
    }
    /// <summary>
    /// rfc1094[1989.3]-[2.3.5]-[page 15]
    /// rfc1094中的 "fattr" 结构，标识文件的属性
    /// </summary>
    public class NfsFileAttribute
    {
        public readonly NfsFileType Type = NfsFileType.NFREG;
        //文件访问权限及模式
        public NfsFileMode Mode = NfsFileMode.OWNEXEC | NfsFileMode.GROUPEXEC | NfsFileMode.WORLDEXEC |
            NfsFileMode.OWNREAD | NfsFileMode.GROUPREAD | NfsFileMode.WORLDREAD;
        public readonly int Nlink = 1;  //the number of different names for the same file   ???
        public readonly int Uid = 0;    //user id
        public readonly int Gid = 0;    //group id
        public readonly int Size = 0;   //文件大小
        public const int BlockSize = 4096;   //文件块的大小。默认4Kb
        public readonly int Rdev = 0;   //
        public readonly int Blocks = 0; //文件块的数量
        public readonly int FileSysId = 1;   //拥有该文件的FileSys id
        public readonly int FileId = 0; //FileSys内该文件的唯一标识
        public readonly DateTime LastAccessTime;
        public readonly DateTime LastModifiedTime;
        public readonly DateTime LastChangedTime;
        public readonly bool IsFile = true;

        public NfsFileAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileNotFoundException();
            if (!File.Exists(path) && !Directory.Exists(path))
                throw new FileNotFoundException();
            var info = new FileInfo(path);
            var isDir = info.Attributes.HasFlag(FileAttributes.Directory);
            if (isDir)
            {
                Type = NfsFileType.NFDIR;
                Mode |= NfsFileMode.DIR;
                IsFile = false;
            }
            else
            {
                if (info.Extension.ToLower().Equals(".sl"))
                {
                    Type = NfsFileType.NFLNK;
                    Mode |= NfsFileMode.LNK;
                }
                else
                {
                    Type = NfsFileType.NFREG;
                    Mode |= NfsFileMode.FILE;
                }
            }
            if (!info.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                Mode |= NfsFileMode.GROUPWRITE | NfsFileMode.OWNWRITE | NfsFileMode.WORLDWRITE;
            }
            Size = isDir ? BlockSize : (int)info.Length;
            var rem = 0;
            Blocks = Math.DivRem(Size, BlockSize, out rem);
            if (rem > 0)
                Blocks += 1;
            Nlink = isDir ? 2 : 1;  //文件夹通常有2个link ？？
            FileSysId = isDir ? 0 : 1;
            FileId = path.GetHashCode();
            LastAccessTime = info.LastAccessTime;
            LastModifiedTime = info.LastWriteTime;
            LastChangedTime = info.LastWriteTime;
        }
        public void AppendToRpcPacket(RpcPacket packet)
        {
            packet.Append((int)Type);
            packet.Append((int)Mode);
            packet.Append(Nlink);
            packet.Append(Uid);
            packet.Append(Gid);
            packet.Append(Size);
            packet.Append(NfsFileAttribute.BlockSize);
            packet.Append(Rdev);
            packet.Append(Blocks);
            packet.Append(FileSysId);
            packet.Append(FileId);
            packet.Append(LastAccessTime);
            packet.Append(LastModifiedTime);
            packet.Append(LastChangedTime);
        }
    }
    /// <summary>
    /// rfc1094[1989.3]-[2.3.6]-[page 17]
    /// rfc1094中的 "sattr" 结构，表示可从客户端设置的文件属性
    /// </summary>
    public class NfsFileAttributeSetting
    {
        public int Mode { get; set; }
        public int Uid { get; set; }
        public int Gid { get; set; }
        /// <summary>
        /// A "size" of zero means the file should be truncated. A value of -1 indicates a field that should be ignored.
        /// </summary>
        public int Size { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        private NfsFileAttributeSetting() { }
        public static NfsFileAttributeSetting FromRpcPacket(RpcPacket packet)
        {
            var setting = new NfsFileAttributeSetting();
            setting.Mode = packet.GetInt32();
            setting.Uid = packet.GetInt32();
            setting.Gid = packet.GetInt32();
            setting.Size = packet.GetInt32();
            setting.LastAccessTime = packet.GetDateTime();
            setting.LastModifiedTime = packet.GetDateTime();
            return setting;
        }
    }
}
