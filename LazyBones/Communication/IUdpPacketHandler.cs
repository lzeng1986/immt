using System;
using System.Net;

namespace LazyBones.Communication
{
    public interface IUdpPacketHandler
    {
        byte[] Process(IPEndPoint remoteIPEP, byte[] data);
    }
}
