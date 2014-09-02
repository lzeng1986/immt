using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LazyBones.Communication.Apps.NfsV2
{
    // nfs服务根据 rfc1094 实现 [10/2/2013 zliang]
    // 这里将nfs服务和mount服务集成在一起实现，使用同一个端口
    // mount ->程序号：100005 ** 版本号：1   rfc1094-[Page-25]
    // nfs   ->程序号：100003 ** 版本号：2   rfc1094-[Page-4~5]
    // 程序号和版本号均来自rfc1094
    public class NfsV2Session : RpcV2Session
    {
        /* The maximum number of bytes in a pathname argument. */
        const int MaxPathLength = 1024;
        /* The maximum number of bytes in a name argument. */
        const int MaxNameLength = 255;
        /* The size in bytes of the opaque file handle. */
        const int FileHandleSize = 32;
        const int True = 1;
        const int False = 0;

        protected override int CheckProgAndVer(int progId, int progVer)
        {
            if (progId.Equals(100005))
            {
                return progVer.Equals(1) ? -1 : 1;
            }
            else if (progId.Equals(100003))
            {
                return progVer.Equals(2) ? -1 : 2;
            }
            return -2;
        }
        protected override void Call(int progId, int procedureId, RpcPacket rpcPacket, RpcPacket replyPacket)
        {
            switch (progId)
            {
                case 100003:
                    //ProcessNfsSvc(procedureId, rpcPacket, replyPacket);
                    break;
                case 100005:
                    //ProcessMountSvc(procedureId, rpcPacket, replyPacket);
                    break;
            }
        }

        Dictionary<int, Action<RpcPacket, RpcPacket>> mountMapper = new Dictionary<int, Action<RpcPacket, RpcPacket>>();
        Dictionary<int, Action<RpcPacket, RpcPacket>> nfsMapper = new Dictionary<int, Action<RpcPacket, RpcPacket>>();
        public override void Initialize()
        {
            mountMapper = new Dictionary<int, Action<RpcPacket, RpcPacket>>
            {
                {MountProcedure.Null,NULL},
                //If the reply "status" is 0, then the reply "directory" contains the file handle for the directory "dirname".
            //This file handle may be used in the NFS protocol. This procedure also adds a new entry to the mount list for this client mounting "dirname".
                {MountProcedure.Mount,Mount},
                {MountProcedure.UMount,UMount},
                {MountProcedure.Export,Export},
            };

            //nfsProcessor[-1] = (call, reply) => { throw new BadProcedureException(); };
            //nfsProcessor[NfsV2Procedure.NULL] = (call, reply) => { };
            //nfsProcessor[NfsV2Procedure.GetAttribute] = GetAttribute;
            //nfsProcessor[NfsV2Procedure.SetAttribute] = SetAttribute;
            //nfsProcessor[NfsV2Procedure.LookUp] = LookUp;
            //nfsProcessor[NfsV2Procedure.ReadFile] = ReadFile;
            //nfsProcessor[NfsV2Procedure.WriteFile] = WriteFile;
            //nfsProcessor[NfsV2Procedure.CreateFile] = CreateFile;
            //nfsProcessor[NfsV2Procedure.ReadDir] = ReadFromDir;
            //nfsProcessor[NfsV2Procedure.StateFileSystem] = GetFileSystemInfo;

        }
        void ProcessMountSvc(int procedureId, RpcPacket rpcPacket, RpcPacket replyPacket)
        {
            Action<RpcPacket, RpcPacket> handle;
            if (mountMapper.TryGetValue(procedureId, out handle))
            {
                handle(rpcPacket, replyPacket);
            }
            else
            {
                throw new BadProcedureException();
            }
        }
        static void NULL(RpcPacket callMsg, RpcPacket reply)//无动作。只是为了测试服务响应和测试时间
        {
        }
        void Mount(RpcPacket callMsg, RpcPacket reply)
        {
            var dirPath = callMsg.GetString();
            var server = OperationContext.Current.GetAppServerInstance<NfsV2Server>();
            var export = server.Exports.Find(e => e.ExportName == dirPath);
            if (export != null)
            {
                try
                {
                    //reply.Append((int)NfsStatus.OK);
                    //var root = GetRoot(callMsg.RemoteIPEP.Address);// 这里获取当前用户发送根目录 [10/31/2013 zliang]
                    //var id = fileSystem[root];
                    //var fileHandle = new FileHandle(id);
                    //fileHandle.AppendToRpcPacket(reply);
                }
                catch (FileNotFoundException)
                {
                    reply.Append((int)NfsStatus.NotExist);
                }
            }
            else
            {
                reply.Append((int)NfsStatus.NotExist);
            }
        }

        void UMount(RpcPacket callMsg, RpcPacket reply)
        {
            var dirPath = callMsg.GetString();
            reply.Append((int)NfsStatus.OK);
        }
        void Export(RpcPacket callMsg, RpcPacket reply)
        {
            var server = OperationContext.Current.GetAppServerInstance<NfsV2Server>();
            server.Exports.ForEach(e =>
            {
                reply.Append(True);
                reply.Append(e.ExportName);
            });
            reply.Append(False);
        }
        //void ProcessNfsSvc(int procedureId, RpcPacket rpcPacket, RpcPacket replyPacket)
        //{
        //    try
        //    {
        //        if (nfsProcessor.ContainsKey(procedureId))
        //        {
        //            nfsProcessor[procedureId](rpcPacket, replyPacket);
        //        }
        //        else
        //        {
        //            nfsProcessor[-1](rpcPacket, replyPacket);
        //        }
        //    }
        //    catch (NFSException e)
        //    {
        //        replyPacket.Append((int)e.Status);
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        replyPacket.Append((int)NfsStatus.NotExist);
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        replyPacket.Append((int)NfsStatus.NFSERR_PERM);
        //    }
        //    catch (PathTooLongException)
        //    {
        //        replyPacket.Append((int)NfsStatus.NameTooLong);
        //    }
        //    catch (DirectoryNotFoundException)
        //    {
        //        replyPacket.Append((int)NfsStatus.NotExist);
        //    }
        //    catch (Exception e)
        //    {
        //        replyPacket.Append((int)NfsStatus.IO);
        //    }
        //}
        //void GetAttribute(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    var handle = FileHandle.FromRpcPacket(callMsg);
        //    try
        //    {
        //        var name = fileSystem[handle.Id];
        //        var attr = new NfsFileAttribute(name);
        //        replyPacket.Append((int)NfsStatus.OK);
        //        attr.AppendToRpcPacket(replyPacket);
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        fileSystem.Remove(handle.Id);
        //        throw;
        //    }
        //}
        //void SetAttribute(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    var handle = FileHandle.FromRpcPacket(callMsg);
        //    try
        //    {
        //        var name = fileSystem[handle.Id];
        //        var attrSetting = NfsFileAttributeSetting.FromRpcPacket(callMsg);
        //        replyPacket.Append((int)NfsStatus.OK);
        //        var attr = new NfsFileAttribute(name);
        //        attr.AppendToRpcPacket(replyPacket);
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        fileSystem.Remove(handle.Id);
        //        throw;
        //    }
        //}
        //void LookUp(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    var dirAgrs = DirAgrs.FromRpcPacket(callMsg);
        //    var root = fileSystem[dirAgrs.DirHandle.Id];
        //    var path = dirAgrs.Name != "." ? Path.Combine(root, dirAgrs.Name) : root;
        //    Console.WriteLine("lookup:" + path);
        //    var handle = fileSystem[path];
        //    var attr = new NfsFileAttribute(path);
        //    replyPacket.Append((int)NfsStatus.OK);
        //    var fileHandle = new FileHandle(handle);
        //    fileHandle.AppendToRpcPacket(replyPacket);
        //    attr.AppendToRpcPacket(replyPacket);
        //}
        //void ReadFromDir(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    var fh = FileHandle.FromRpcPacket(callMsg);
        //    var cookie = callMsg.GetInt32();
        //    var count = callMsg.GetInt32();
        //    var rootDir = fileSystem[fh.Id];
        //    Console.WriteLine("ReadFromDir:" + rootDir);
        //    var filePaths = rootDir == "" ? new string[0] : Directory.GetFiles(rootDir);
        //    var fileNames = filePaths.OrderBy(o => o)
        //        .Select((name, i) => new { Ind = i, Name = Path.GetFileName(name), FullName = name })
        //        .Skip(cookie);
        //    replyPacket.Append((int)NfsStatus.OK);
        //    var finished = true;
        //    foreach (var entry in fileNames)
        //    {
        //        int needed = 3 * 4 + (entry.Name.Length + 3) + 8; //检查数据包是否有足够空间存放数据
        //        if (needed + replyPacket.Length >= count)
        //        {
        //            finished = false;
        //            break;
        //        }
        //        var handle = fileSystem[entry.FullName];
        //        replyPacket.Append(True); //Flag
        //        replyPacket.Append(handle);   //Handle，不是FileHandle
        //        replyPacket.Append(entry.Name); //Name
        //        replyPacket.Append(entry.Ind + 1); // this is the cookie
        //    }
        //    replyPacket.Append(False);  //表示该数据包中没有更多的entry
        //    replyPacket.Append(finished ? True : False);   //表示是否有更多的数据，True表示没有更多，False还有数据，需要客户端继续提出申请
        //}
        //void GetFileSystemInfo(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    var fh = FileHandle.FromRpcPacket(callMsg);
        //    var root = Directory.GetDirectoryRoot(fileSystem[fh.Id]);
        //    var rootInfo = new DriveInfo(root);
        //    var totalBlocks = (int)(rootInfo.TotalSize / NfsFileAttribute.BlockSize);
        //    var freeBlocks = (int)(rootInfo.TotalFreeSpace / NfsFileAttribute.BlockSize);
        //    var availableBlocks = (int)(rootInfo.AvailableFreeSpace / NfsFileAttribute.BlockSize);
        //    replyPacket.Append((int)NfsStatus.OK);
        //    replyPacket.Append(NfsFileAttribute.BlockSize);				// tsize: optimum transfer size
        //    replyPacket.Append(NfsFileAttribute.BlockSize);				// Block size of FS
        //    replyPacket.Append(totalBlocks);		// Total # of blocks (of the above size)
        //    replyPacket.Append(freeBlocks);	// Free blocks
        //    replyPacket.Append(availableBlocks);		// Free blocks available to non-priv. users
        //}
        //void ReadFile(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    //readargs
        //    var fh = FileHandle.FromRpcPacket(callMsg);
        //    var offset = callMsg.GetInt32();
        //    var count = callMsg.GetInt32();
        //    var totalCount = callMsg.GetInt32();
        //    var path = fileSystem[fh.Id];
        //    Console.WriteLine("ReadFile:" + path);
        //    try
        //    {
        //        var attr = new NfsFileAttribute(path);
        //        if (!attr.IsFile)
        //            throw new NFSException(NfsStatus.IsDirectory);
        //        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            fs.Position = offset;
        //            var buf = new byte[count];
        //            var bytesRead = fs.Read(buf, 0, count);
        //            replyPacket.Append((int)NfsStatus.OK);
        //            attr.AppendToRpcPacket(replyPacket);
        //            replyPacket.Append(buf, bytesRead);
        //        }
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        fileSystem.Remove(fh.Id);
        //        throw;
        //    }
        //}
        //void WriteFile(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    //writeargs
        //    var fh = FileHandle.FromRpcPacket(callMsg);
        //    var beginoffset = callMsg.GetInt32();
        //    var offset = callMsg.GetInt32();
        //    var totalcount = callMsg.GetInt32();
        //    var data = callMsg.GetData();
        //    var path = fileSystem[fh.Id];
        //    Console.WriteLine("WriteFile:" + path);
        //    try
        //    {
        //        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Write))
        //        {
        //            fs.Position = offset;
        //            fs.Write(data, 0, data.Length);
        //        }
        //        replyPacket.Append((int)NfsStatus.OK);
        //        var attr = new NfsFileAttribute(path);
        //        attr.AppendToRpcPacket(replyPacket);
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        fileSystem.Remove(fh.Id);
        //        throw;
        //    }
        //}
        //void CreateFile(RpcPacket callMsg, RpcPacket replyPacket)
        //{
        //    //writeargs
        //    var dirAgrs = DirAgrs.FromRpcPacket(callMsg);
        //    var attrSetting = NfsFileAttributeSetting.FromRpcPacket(callMsg);
        //    var rootDir = GetRoot(callMsg.RemoteIPEP.Address);// 这里获取当前用户回传根目录 [10/31/2013 zliang]
        //    var path = Path.Combine(rootDir, dirAgrs.Name);
        //    Console.WriteLine("CreateFile:" + path);
        //    using (File.Create(path)) { }
        //    replyPacket.Append((int)NfsStatus.OK);
        //    var handle = fileSystem[path];
        //    var fh = new FileHandle(handle);
        //    fh.AppendToRpcPacket(replyPacket);
        //    var attr = new NfsFileAttribute(path);
        //    attr.AppendToRpcPacket(replyPacket);
        //}
    }
    public class NFSException : Exception
    {
        public readonly NfsStatus Status;
        public NFSException(NfsStatus status)
        {
            this.Status = status;
        }
    }
    public static class NfsV2Procedure
    {
        public const int
            NULL = 0,
            GetAttribute = 1,
            SetAttribute = 2,
            Root = 3,
            LookUp = 4,
            ReadLink = 5,
            ReadFile = 6,
            WriteCache = 7,
            WriteFile = 8,
            CreateFile = 9,
            RemoveFile = 10,
            Rename = 11,
            Link = 12,
            Symlink = 13,
            CreateDir = 14,
            RemoveDir = 15,
            ReadDir = 16,
            StateFileSystem = 17;
    }
    public static class MountProcedure
    {
        public const int
            Null = 0,
            Mount = 1,
            Dump = 2,
            UMount = 3,
            UMountAll = 4,
            Export = 5;
    }
}
