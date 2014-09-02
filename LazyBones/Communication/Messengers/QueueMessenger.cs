using System;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;
using LazyBones.Threading;

namespace LazyBones.Communication.Messengers
{
    public class QueueMessenger
    {
        IMessenger innerMessenger;
        BackgroundQueueWorker sendWorker = new BackgroundQueueWorker(1024);
        BackgroundQueueWorker receiveWorker = new BackgroundQueueWorker(1024);
        public QueueMessenger(IMessenger innerMessenger, int capacity)
        {
            this.innerMessenger = innerMessenger;
            innerMessenger.MessageReceived += innerChannel_MessageReceived;
            innerMessenger.MessageSent += innerChannel_MessageSent;
        }

        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<MessageEventArgs> MessageSent;

        public void SendMessage(ILBMessage message)
        {
            sendWorker.RunAsync(OnSendMessage, message);
        }

        public TransferState TransferState
        {
            get { return innerMessenger.TransferState; }
        }

        void innerChannel_MessageSent(object sender, MessageEventArgs e)
        {
            var handle = MessageSent;
            if (handle != null)
                handle(this, e);
        }

        void innerChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            receiveWorker.RunAsync(OnMessageReceived, e.Message);
        }
        protected virtual void OnSendMessage(ILBMessage message)
        {
            innerMessenger.SendMessage(message);
        }

        protected virtual void OnMessageReceived(ILBMessage message)
        {
            var handle = MessageSent;
            if (handle != null)
                handle(this, new MessageEventArgs(message));
        }
    }
}
