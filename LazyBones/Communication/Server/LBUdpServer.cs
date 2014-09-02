using System.Collections.Generic;
using LazyBones.Communication.Channels;

namespace LazyBones.Communication.Server
{
    class LBUdpServer : LBServerBase
    {
        Dictionary<string, UdpChannel> channels = new Dictionary<string, UdpChannel>();

        protected override ILBChannelListener CreateChannelListener()
        {
            return new UdpChannelListener(ListenEndPoint);
        }

        protected override ChannelBase CreateChannel(ConnectionEventArgs e)
        {
            lock (channels)
            {
                var info = (UdpPacket)e.State;
                var id = info.RemoteEndPoint.ToString();
                UdpChannel channel;
                if (!channels.TryGetValue(id, out channel))
                {
                    channel = new UdpChannel(e.Socket, info.RemoteEndPoint);
                    channels.Add(id,channel);
                }
                channel.ReceiveData = info.Data;
                return channel;
            }
        }
    }
}
