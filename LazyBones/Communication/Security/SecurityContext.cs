using System;
using System.Net;
using System.Security.Principal;

namespace LazyBones.Communication.Security
{
    public class SecurityContext
    {
        [ThreadStatic]
        internal static SecurityContext current;
        public static SecurityContext Current { get { return current; } }

        public IIdentity Identity { get; internal set; }
        public WindowsIdentity WinIdentity { get; internal set; }
        public IPEndPoint RemoteIPEndPoint { get; internal set; }
    }
}
