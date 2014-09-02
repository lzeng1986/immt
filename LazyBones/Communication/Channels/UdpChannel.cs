using System;
using System.Net;
using System.Net.Sockets;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Channels
{
    class UdpChannel : ChannelBase
    {
        Socket udpSocket;
        public UdpChannel(Socket udpSocket, IPEndPoint remoteIPEP)
        {
            this.udpSocket = udpSocket;
            RemoteEndPoint = remoteIPEP;
        }
        public byte[] ReceiveData { get; set; }
        protected override void OnSend(byte[] bytes)
        {
            udpSocket.SendTo(bytes, RemoteEndPoint);
        }

        protected override void OpenCommunicator()
        {
            ReceiveBytes(new ArraySegment<byte>(ReceiveData));
        }

        protected override void CloseCommunicator()
        {
            udpSocket = null;
        }
    }
}
