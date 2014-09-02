using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using LazyBones.Communication.Core;

namespace LazyBones.Communication.Channels
{
    class UdpChannelListener : ChannelListenerBase
    {
        Socket listenerSocket;
        EndPoint listenerEndPoint;
        SocketAsyncEventArgs asycnArgs;
        public ArraySegment<byte> ReceiveBuffer { get; set; }
        public UdpChannelListener(EndPoint endPoint)
        {
            listenerEndPoint = endPoint;
        }
        protected override void OpenCommunicator()
        {
            try
            {
                asycnArgs = new SocketAsyncEventArgs();
                asycnArgs.Completed += asycnArgs_Completed;
                listenerSocket = new Socket(listenerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listenerSocket.Bind(listenerEndPoint);

                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { 0 };
                byte[] optionOutValue = new byte[4];
                listenerSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                asycnArgs.SetBuffer(ReceiveBuffer.Array, ReceiveBuffer.Offset, ReceiveBuffer.Count);
                Reveive(asycnArgs);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
        void Reveive(SocketAsyncEventArgs e)
        {
            if (!listenerSocket.ReceiveFromAsync(e))
                Process(e);
        }
        void asycnArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            Process(e);
        }
        void Process(SocketAsyncEventArgs e)
        {
            if (e.IsSuccess() && e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                var buf = new byte[e.BytesTransferred];
                var ep = e.RemoteEndPoint as IPEndPoint;
                Buffer.BlockCopy(e.Buffer, 0, buf, 0, e.BytesTransferred);
                OnConnectionAccepted(listenerSocket, new UdpPacket(ep, buf));
                Reveive(asycnArgs);
            }
        }
        protected override void CloseCommunicator()
        {
            listenerSocket.SafeClose();
            asycnArgs.Dispose();
            asycnArgs = null;
        }
    }
}
