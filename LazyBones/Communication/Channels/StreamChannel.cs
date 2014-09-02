using System;
using System.IO;
using System.Net.Sockets;
using LazyBones.Communication.Messages;
using LazyBones.Log;

namespace LazyBones.Communication.Channels
{
    abstract class StreamChannel : ChannelBase
    {
        static Logger logger = LogManager.Current;
        protected Stream stream;
        protected Socket clientSocket;
        object sendLocker = new object();

        public StreamChannel(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }
        protected override void OnSend(byte[] bytes)
        {
            lock (sendLocker)
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        protected void Receive()
        {
            try
            {
                stream.BeginRead(ReceiveBuffer.Array, ReceiveBuffer.Offset, ReceiveBuffer.Count, EndReceive, null);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                OnError(e);
            }
        }
        void EndReceive(IAsyncResult result)
        {
            try
            {
                var size = stream.EndRead(result);
                if (size == 0)
                {
                    Close();
                    return;
                }

                ReceiveBytes(new ArraySegment<byte>(ReceiveBuffer.Array, ReceiveBuffer.Offset, size));
                Receive();
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                OnError(e);
            }
        }
        protected override void CloseCommunicator()
        {
            stream.Close();
        }
    }
}
