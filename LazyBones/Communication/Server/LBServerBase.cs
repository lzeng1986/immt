using System.Collections.Generic;
using System.Net;
using LazyBones.Communication.Channels;
using System.Threading;
using System;

namespace LazyBones.Communication.Server
{
    public abstract class LBServerBase : ILBServer
    {
        Dictionary<string, ILBSessionContext> sessions = new Dictionary<string, ILBSessionContext>();
        BufferManager receiveBuffers;
        BufferManager deSerializeBuffers;

        public EndPoint ListenEndPoint { get; internal set; }

        public LBBinding Binding { get; internal set; }
        public IAppServer AppServer { get; set; }
        ILBChannelListener channelListener;

        void channelListener_NewConnectionAccepted(object sender, ConnectionEventArgs e)
        {
            var newChannel = (ChannelBase)CreateChannel(e);
            if (newChannel == null)
                return;
            if (limiter.WaitOne(Binding.ConnectTimeOut))//通过信号量客户端限制最大连接数
                AcceptNewClient(newChannel);
            else
                newChannel.Close();
        }
        void AcceptNewClient(ChannelBase newChannel)
        {
            newChannel.Closed += newChannel_Closed;
            newChannel.Error += newChannel_Error;
            newChannel.Binding = Binding;
            deSerializeBuffers.TryGet(out newChannel.DeserializeBuffer);
            var protocol = Binding.CreateProtocol();
            newChannel.Protocol = protocol;
            var newSession = new ServerClientSession(newChannel);
            newSession.SessionId = AppServer.GetNewClientId(newChannel.RemoteEndPoint);
            using (OperationContext.SwichContext(newSession))
            {
                var appSession = Binding.CreateAppSession(newSession);
                newSession.SetAppSession(appSession);
                sessions[newSession.SessionId] = newSession;
                newSession.IdleSessionTimeOut = Binding.IdleSessionTimeOut;
                newSession.AppServer = AppServer;
                newSession.Open();
            }
        }

        void newChannel_Error(object sender, ErrorEventArgs e)
        {
            CloseChannel(sender as ChannelBase);
        }

        void newChannel_Closed(object sender, EventArgs e)
        {
            CloseChannel(sender as ChannelBase);
        }
        void CloseChannel(ChannelBase channel)
        {
            limiter.Release();
            channel.Closed -= newChannel_Closed;
            channel.Error -= newChannel_Error;
        }
        protected abstract ILBChannelListener CreateChannelListener();
        protected abstract ChannelBase CreateChannel(ConnectionEventArgs e);
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

        public Dictionary<string, ILBSessionContext> Clients
        {
            get { return sessions; }
        }
        Semaphore limiter;
        public void Start()
        {
            channelListener = CreateChannelListener();
            channelListener.NewConnectionAccepted += channelListener_NewConnectionAccepted;
            channelListener.Open();
            limiter = new Semaphore(Binding.MaxConnection, Binding.MaxConnection);
            deSerializeBuffers = new BufferManager(Binding.MaxConnection, Binding.MaxGraphObjectSize);
            receiveBuffers = new BufferManager(Binding.MaxConnection, Binding.ReceiveBufferSize);
            OnStart();
        }

        public void Stop()
        {
            if (channelListener != null)
            {
                channelListener.Close();
                channelListener = null;
                OnStop();
                limiter.Close();
                deSerializeBuffers.Dispose();
                receiveBuffers.Dispose();
            }
        }
    }
}
