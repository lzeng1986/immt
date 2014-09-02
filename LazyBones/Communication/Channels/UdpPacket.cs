using System.Net;

namespace LazyBones.Communication.Channels
{
    //程序集内部使用
    struct UdpPacket
    {
        public UdpPacket(IPEndPoint remoteEndPoint, byte[] data)
        {
            this.remoteEndPoint = remoteEndPoint;
            this.data = data;
        }
        IPEndPoint remoteEndPoint;
        public IPEndPoint RemoteEndPoint { get { return remoteEndPoint; } }
        byte[] data;
        public byte[] Data { get { return data; } }
    }
}
