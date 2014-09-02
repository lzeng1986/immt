using System;
using System.Net;
using LazyBones.Communication.Server;

namespace LazyBones.Communication
{
    /// <summary>
    /// 用于承载服务，这是这个通讯框架的用户入口
    /// </summary>
    public class ServerHost : Communicator
    {
        LBServerBase server;
        IAppServer appServer;
        EndPoint listenEndpoint;
        public ServerHost(IAppServer appServer, EndPoint endpoint)
        {
            CreateTime = DateTimeOffset.Now;
            this.appServer = appServer;
            listenEndpoint = endpoint;
            server = CreateServer(appServer.Binding);
            server.Binding = appServer.Binding;
            server.ListenEndPoint = endpoint;
        }

        public DateTimeOffset CreateTime { get; private set; }
        public DateTimeOffset StartTime { get; private set; }

        LBServerBase CreateServer(LBBinding binding)
        {
            switch (binding.SocketMode)
            {
                case SocketMode.Tcp:
                    return new LBTcpServer();
                case SocketMode.Udp:
                    return new LBUdpServer();
                default:
                    throw new ArgumentException();
            }
        }

        protected override void OpenCommunicator()
        {
            StartTime = DateTimeOffset.Now;
            server.Start();
        }

        protected override void CloseCommunicator()
        {
            server.Stop();
        }
    }

}
