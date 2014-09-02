using System;

namespace LazyBones.Communication.Messages
{
    public class MessageEventArgs : EventArgs
    {
        public ILBMessage Message { get; private set; }

        public MessageEventArgs(ILBMessage message)
        {
            Message = message;
        }
    }

    public class MessageEventArgs<TMessage> : EventArgs
        where TMessage : ILBMessage
    {
        public TMessage Message { get; private set; }

        public MessageEventArgs(TMessage message)
        {
            Message = message;
        }
    }
}
