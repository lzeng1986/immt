using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Threading;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Channels
{
    public class QueueChannel<TChannel> : Communicator, ILBChannel
        where TChannel : ILBChannel
    {
        ILBChannel innerChannel;
        BackgroundQueueWorker backWorker;
        public QueueChannel(ILBChannel innerChannel)
        {
            this.innerChannel = innerChannel;
        }

        protected override void OpenCommunicator()
        {
            innerChannel.Open();
        }

        protected override void CloseCommunicator()
        {
            innerChannel.Close();
        }

        public event EventHandler<Messages.MessageEventArgs> MessageReceived;

        public event EventHandler<Messages.MessageEventArgs> MessageSent;

        public void SendMessage(Messages.ILBMessage message)
        {
            throw new NotImplementedException();
        }

        public LBProtocol Protocol
        {
            get { return innerChannel.Protocol; }
        }

        public int IncomingMessageQueueCapacity { get; set; }

        public TransferState TransferState
        {
            get { return innerChannel.TransferState; }
        }

        public System.Net.IPEndPoint RemoteEndPoint
        {
            get { return innerChannel.RemoteEndPoint; }
        }
    }
}
