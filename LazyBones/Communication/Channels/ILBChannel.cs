using System.Net;
using LazyBones.Communication.Messengers;
using System;

namespace LazyBones.Communication.Channels
{
    public interface ILBChannel : ICommunicator, IMessenger
    {
        IPEndPoint RemoteEndPoint { get; }
        event EventHandler<TransferStateChangedEventArgs> TransferStateChanged;
    }
    public class TransferStateChangedEventArgs : EventArgs
    {
        public TransferState OldState { get; private set; }
        public TransferState NewState { get; private set; }
        public TransferStateChangedEventArgs(TransferState old, TransferState @new)
        {
            OldState = old;
            NewState = @new;
        }
    }
}
