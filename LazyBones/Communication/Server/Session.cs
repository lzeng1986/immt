using System;
using System.Net;
using LazyBones.Communication.Channels;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Server
{
    class Session<TMessage> : IOperation
        where TMessage : class,ILBMessage
    {
        ILBChannel communicationChannel;

        IAppSession<TMessage> appSession;

        internal AppServer AppServer;

        internal Session(ILBChannel channel, IAppSession<TMessage> appSession)
        {
            communicationChannel = channel;
            this.appSession = appSession;
            channel.MessageReceived += new EventHandler<MessageEventArgs>(channel_MessageReceived);
            channel.MessageSent += new EventHandler<MessageEventArgs>(channel_MessageSent);
            RemoteEndPoint = channel.RemoteEndPoint;
            LastReceiveTime = DateTime.MinValue;
            LastSentTime = DateTime.MinValue;
            SessionCreateTime = DateTimeOffset.Now;
        }

        void channel_MessageSent(object sender, MessageEventArgs e)
        {
            var msg = e.Message as TMessage;
            if (msg != null)
            {
                LastSentTime = DateTime.Now;
                appSession.MessageSent(msg);
            }
        }

        void channel_MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message as TMessage;
            if (msg != null)
            {
                LastReceiveTime = DateTime.Now;
                appSession.Process(msg);
            }
        }

        public IServer Server { get; internal set; }

        public string SessionId { get; internal set; }

        public TAppServer GetServer<TAppServer>() 
            where TAppServer : AppServer
        {
            return AppServer as TAppServer;
        }

        public void SendMessage(ILBMessage message)
        {
            communicationChannel.SendMessage(message);
        }

        public LBProtocol Protocol
        {
            get { return communicationChannel.Protocol; }
        }

        public DateTimeOffset SessionCreateTime { get; private set; }

        public DateTime LastReceiveTime { get; private set; }

        public DateTime LastSentTime { get; private set; }

        public TransferState TransferState
        {
            get { return communicationChannel.TransferState; }
        }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public void Open()
        {
            communicationChannel.Open();
        }

        public void Close()
        {
            communicationChannel.Close();
        }
    }
}
