using System;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Messengers
{
    /// <summary>
    /// 定义发送接收消息的接口
    /// </summary>
    public interface IMessenger
    {
        event EventHandler<MessageEventArgs> MessageReceived;
        event EventHandler<MessageEventArgs> MessageSent;
        TransferState TransferState { get; }
        void SendMessage(ILBMessage message);
    }
}
