using System;

namespace LazyBones.Communication.Apps.Ftp
{
    [Flags]
    public enum UserPermissions
    {
        StoreFile = 0x01,
        StoreFloder = 0x02,
        RenameFile = 0x04,
        RenameFloder = 0x08,
        DeleteFile = 0x10,
        DeleteFloder = 0x2,
        CopyFile = 0x40,
        ViewHideFile = 0x80,
        ViewHideFloder = 0x100,
        DownFile = 0x200,
        Default = DownFile,
        FullControl = 0x3FF
    }
}
