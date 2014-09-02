using System;
using System.Net.Sockets;
using LazyBones.Extensions;

namespace LazyBones.Communication.Channels
{
    abstract class ChannelListenerBase : Communicator, ILBChannelListener
    {
        public event EventHandler<ConnectionEventArgs> NewConnectionAccepted;

        protected virtual void OnConnectionAccepted(Socket socket, object state)
        {
            NewConnectionAccepted.SafeCall(this, new ConnectionEventArgs(socket, state));
        }
    }
}
