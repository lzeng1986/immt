using System;
using System.Net;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Server
{
    public interface ILBSessionContext
    {
        string SessionId { get; }
        DateTimeOffset SessionCreateTime { get; }
        DateTime LastReceiveTime { get; }
        DateTime LastSentTime { get; }
        TransferState TransferState { get; }
        IPEndPoint RemoteEndPoint { get; }
        void SendMessage(ILBMessage message);
        TAppServer GetAppServerInstance<TAppServer>() where TAppServer : IAppServer;
    }
}
