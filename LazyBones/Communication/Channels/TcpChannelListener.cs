using System.Net;
using System.Net.Sockets;
using LazyBones.Communication.Core;

namespace LazyBones.Communication.Channels
{
    class TcpChannelListener : ChannelListenerBase
    {
        Socket listenerSocket;
        EndPoint listenerEndPoint;
        int backlog;
        SocketAsyncEventArgs asycnArgs;
        public TcpChannelListener(EndPoint endPoint, int backlog)
        {
            listenerEndPoint = endPoint;
            this.backlog = backlog;
        }
        protected override void OpenCommunicator()
        {
            asycnArgs = new SocketAsyncEventArgs();
            asycnArgs.Completed += asycnArgs_Completed;
            listenerSocket = new Socket(listenerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.Bind(listenerEndPoint);
            listenerSocket.Listen(backlog);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            if (!listenerSocket.AcceptAsync(asycnArgs))
                Process(asycnArgs);
        }
        void asycnArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            Process(e);
        }
        void Process(SocketAsyncEventArgs e)
        {
            if (e.IsSuccess() && e.LastOperation == SocketAsyncOperation.Accept)
            {
                var socket = e.AcceptSocket;
                e.AcceptSocket = null;
                if (socket != null)
                    OnConnectionAccepted(socket, null);
                if (!listenerSocket.AcceptAsync(asycnArgs))
                    Process(asycnArgs);
            }
            else
            {
                Close();
            }
        }
        protected override void CloseCommunicator()
        {
            listenerSocket.SafeClose();
        }
    }
}
