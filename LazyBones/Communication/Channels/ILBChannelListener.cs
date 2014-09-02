using System;

namespace LazyBones.Communication.Channels
{
    public interface ILBChannelListener : ICommunicator
    {
        event EventHandler<ConnectionEventArgs> NewConnectionAccepted;
    }
}
