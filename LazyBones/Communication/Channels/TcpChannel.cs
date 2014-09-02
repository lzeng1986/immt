using System;
using System.Net;
using System.Net.Sockets;
using LazyBones.Communication.Core;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Channels
{
    class TcpChannel : ChannelBase
    {
        Socket clientSocket;
        SocketAsyncEventArgs receiveSocketEventArgs;
        
        object sendLocker = new object();
        public TcpChannel(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            RemoteEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
        }
        protected override void OnSend(byte[] bytes)
        {
            lock (sendLocker)
            {
                var left = bytes.Length;
                var sent = 0;
                while (left > 0)
                {
                    var n = clientSocket.Send(bytes, sent, left, SocketFlags.None);
                    left -= n;
                    sent += n;
                }
            }
        }
        protected override void OpenCommunicator()
        {
            var v = new int[] { 1, 1000, Binding.KeepAliveTimeOut };
            var keepAliveValues = new byte[3 * sizeof(int)];
            Buffer.BlockCopy(v, 0, keepAliveValues, 0, keepAliveValues.Length);
            clientSocket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);
            clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            receiveSocketEventArgs = new SocketAsyncEventArgs();
            receiveSocketEventArgs.SetBuffer(ReceiveBuffer.Array, ReceiveBuffer.Offset, ReceiveBuffer.Count);
            receiveSocketEventArgs.Completed += ReceiveCompleted;
            Receive(receiveSocketEventArgs);
        }
        void Receive(SocketAsyncEventArgs e)
        {
            if (!clientSocket.ReceiveAsync(e))
                ProcessReceive(receiveSocketEventArgs);
        }
        void ProcessReceive(SocketAsyncEventArgs e)
        {
            
            if (e.IsSuccess() && e.LastOperation == SocketAsyncOperation.Receive)
            {
                if (e.BytesTransferred == 0)
                {
                    Close();
                }
                else
                {
                    var receivedBytes = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);
                    ReceiveBytes(new ArraySegment<byte>(receivedBytes));
                    Receive(e);
                }
            }
            else
            {
                Close();
            }
        }
        void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }
        protected override void CloseCommunicator()
        {
            receiveSocketEventArgs.Completed -= ReceiveCompleted;
            receiveSocketEventArgs.SetBuffer(null, 0, 0);
            receiveSocketEventArgs.Dispose();
            receiveSocketEventArgs = null;
            clientSocket.SafeClose();
        }
    }
}
