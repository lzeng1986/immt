using System;
using System.Net;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication
{
    public interface IOperation
    {
        DateTimeOffset SessionCreateTime { get; }
        DateTime LastReceiveTime { get; }
        DateTime LastSentTime { get; }
        TransferState TransferState { get; }
        IPEndPoint RemoteEndPoint { get; }
        void SendMessage(ILBMessage message);
        TAppServer GetServer<TAppServer>() where TAppServer : AppServer;
    }
}
