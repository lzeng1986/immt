using System;
using System.Linq.Expressions;
using System.Net;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Server
{
    public class LBServerBuilder<TMessage>
        where TMessage : class,ILBMessage
    {
        LBServerBase<TMessage> server;
        internal LBServerBuilder(SocketMode mode)
        {
            switch (mode)
            {
                case SocketMode.Tcp:
                    server = new LBTcpServer<TMessage>();
                    break;
                case SocketMode.Udp:
                    server = new LBUdpServer<TMessage>();
                    break;
            }
        }

        public SocketMode BuildServerSocketMode
        {
            get { return server.SocketMode; }
        }

        public LBServerBuilder<TMessage> UsingProtocol(LBProtocol protocol)
        {
            server.Protocol = protocol;
            return this;
        }

        public LBServerBuilder<TMessage> RegisterAppSession<TSession>()
            where TSession : class,IAppSession<TMessage>, new()
        {
            server.AppSessionCreater = () => new TSession();
            return this;
        }

        public LBServerBuilder<TMessage> RegisterAppSession<TSession>(Expression<Func<TSession>> sessionCreater)
            where TSession : class,IAppSession<TMessage>
        {
            server.AppSessionCreater = () => sessionCreater.Compile()();
            return this;
        }

        public LBServerBuilder<TMessage> WithSessionIdGenerator(Func<string> sessionIdGenerator)
        {
            server.SessionIdGenerator = sessionIdGenerator;
            return this;
        }

        public LBServerBuilder<TMessage> ListenAt(EndPoint endpoint)
        {
            server.ListenEndPoint = endpoint;
            return this;
        }
        public LBServerBuilder<TMessage> UsingSyncSend(bool syncSend)
        {
            server.SyncSend = syncSend;
            return this;
        }
        public LBServerBuilder<TMessage> SetTcpBacklog(int backlog)
        {
            server.Backlog = backlog;
            return this;
        }

        public LBServerBuilder<TMessage> WithMaxConnection(int maxConnection)
        {
            server.MaxConnection = maxConnection;
            return this;
        }

        public LBServerBuilder<TMessage> SetReceiveBufferSize(int bufferSize)
        {
            server.ReceiveBufferSize = bufferSize;
            return this;
        }

        public LBServerBuilder<TMessage> SetSendBufferSize(int bufferSize)
        {
            server.SendBufferSize = bufferSize;
            return this;
        }

        public LBServerBuilder<TMessage> SetReceiveTimeOut(int timeout)
        {
            server.ReceiveTimeOut = timeout;
            return this;
        }

        public LBServerBuilder<TMessage> SetSendTimeOut(int timeout)
        {
            server.SendTimeOut = timeout;
            return this;
        }

        public LBServerBuilder<TMessage> SetIdleSessionTimeOut(int timeout)
        {
            server.IdleSessionTimeOut = timeout;
            return this;
        }

        public LBServerBuilder<TMessage> SetKeepAliveTimeOut(int timeout)
        {
            server.KeepAliveTimeOut = timeout;
            return this;
        }

        public LBServerBuilder<TMessage> SetMaxRequestLength(int requestLength)
        {
            server.MaxRequestLength = requestLength;
            return this;
        }

        public TAppServer Build<TAppServer>()
            where TAppServer : AppServer, new()
        {
            if (server.ListenEndPoint == null)
                throw new ArgumentNullException("ListenEndPoint", "需调用ListenAt设置ListenEndPoint");
            if (server.Protocol == null)
                throw new ArgumentNullException("Protocol", "需调用UsingProtocol设置Protocol");
            if (server.AppSessionCreater == null)
                throw new InvalidOperationException("没有调用RegisterAppSession");

            var appserver = new TAppServer();
            server.AppServer = appserver;
            appserver.Initialize(server);
            return appserver;
        }
    }
}
