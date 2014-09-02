using System;
using LazyBones.Communication.Channels;

namespace LazyBones.Communication.Server
{
    class LBTcpServer: LBServerBase
    {
        BufferManager bufferManager;
        protected override void OnStart()
        {
            bufferManager = new BufferManager(Binding.MaxConnection, Binding.ReceiveBufferSize);
        }
        protected override void OnStop()
        {
            bufferManager.Dispose();
            bufferManager = null;
        }
        protected override ILBChannelListener CreateChannelListener()
        {
            return new TcpChannelListener(ListenEndPoint, Binding.Backlog);
        }

        protected override ChannelBase CreateChannel(ConnectionEventArgs e)
        {
            ArraySegment<byte> buffer;
            if (bufferManager.TryGet(out buffer))
            {
                var channel = new TcpChannel(e.Socket);
                channel.ReceiveBuffer = buffer;
                channel.Closing += channel_Closing;
                return channel;
            }
            e.Socket.SafeClose();
            return null;
        }

        void channel_Closing(object sender, EventArgs e)
        {
            var channel = sender as TcpChannel;
            bufferManager.Release(channel.ReceiveBuffer);
        }
    }
}
