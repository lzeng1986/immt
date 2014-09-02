using System;
using System.Net;
using System.Threading;
using LazyBones.Communication.Channels;
using LazyBones.Communication.Config;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Server
{
    //程序集内部用于管理客户端的会话和AppSession的生命周期
    sealed class ServerClientSession : ILBSessionContext, IDisposable
    {
        ILBChannel communicationChannel;
        Timer sessionIdleTimer;
        IAppSession appSession;
        public ServerClientSession(ILBChannel communicationChannel)
        {
            this.communicationChannel = communicationChannel;
            communicationChannel.MessageReceived += channel_MessageReceived;
            communicationChannel.MessageSent += channel_MessageSent;
            communicationChannel.Closed += communicationChannel_Closed;
            RemoteEndPoint = communicationChannel.RemoteEndPoint;
            LastReceiveTime = DateTime.MinValue;
            LastSentTime = DateTime.MinValue;
            SessionCreateTime = DateTimeOffset.Now;
            IdleSessionTimeOut = Defaults.IdleSessionTimeOut;
            sessionIdleTimer = new Timer(TimerCallBack, null, 0, 0);
        }
        public ILBChannel CommunicationChannel
        {
            get { return communicationChannel; }
        }
        public void SetAppSession(IAppSession appSession)
        {
            this.appSession = appSession;
        }
        public IAppServer AppServer { get; set; }
        public void Open()
        {
            SetTimer();
            using (OperationContext.SwichContext(this))
            {
                appSession.Initialize();
            }
        }

        public void Close()
        {
            sessionIdleTimer.Dispose();
            using (OperationContext.SwichContext(this))
            {
                appSession.Dispose();
            }
        }

        void communicationChannel_Closed(object sender, EventArgs e)
        {
            Close();
        }

        void TimerCallBack(object state)
        {
            Close();
        }

        void SetTimer()
        {
            sessionIdleTimer.Change(IdleSessionTimeOut, 0);
        }

        void channel_MessageSent(object sender, MessageEventArgs e)
        {
            SetTimer();
            using (OperationContext.SwichContext(this))
            {
                LastSentTime = DateTime.Now;
                appSession.MessageSent(e.Message);
            }
        }

        void channel_MessageReceived(object sender, MessageEventArgs e)
        {
            SetTimer();
            using (OperationContext.SwichContext(this))
            {
                LastSentTime = DateTime.Now;
                appSession.ProcessMessage(e.Message);
            }
        }

        public string SessionId { get; set; }

        public int IdleSessionTimeOut { get; set; }

        public void SendMessage(ILBMessage message)
        {
            communicationChannel.SendMessage(message);
        }

        public TransferState TransferState
        {
            get { return communicationChannel.TransferState; }
        }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public void Dispose()
        {
            Close();
        }

        public DateTimeOffset SessionCreateTime { get; private set; }

        public DateTime LastReceiveTime { get; private set; }

        public DateTime LastSentTime { get; private set; }

        public TAppServer GetAppServerInstance<TAppServer>() 
            where TAppServer : IAppServer
        {
            return (TAppServer)AppServer;
        }
    }
}
