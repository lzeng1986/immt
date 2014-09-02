using System;
using System.Net;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;
using LazyBones.Extensions;
using LazyBones.Utils;

namespace LazyBones.Communication.Channels
{
    public abstract class ChannelBase : Communicator, ILBChannel
    {
        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<MessageEventArgs> MessageSent;

        internal ILBProtocol Protocol;

        public DateTime CreateTime { get; private set; }

        public DateTime LastReceiveTime { get; private set; }

        public DateTime LastSendTime { get; private set; }

        public LBBinding Binding { get; internal set; }

        public ArraySegment<byte> ReceiveBuffer { get; internal set; }
        internal ArraySegment<byte> DeserializeBuffer;
        int deserializeCount = 0;

        protected ChannelBase()
        {
            CreateTime = DateTime.Now;
            LastReceiveTime = DateTime.MinValue;
            LastSendTime = DateTime.MinValue;
        }

        public void SendMessage(ILBMessage message)
        {
            ParamGuard.NotNull(message, "message");
            TransferState = TransferState.Sending;
            try
            {
                var bytes = Protocol.Serialize(message);
                OnSend(bytes);
                MessageSent.SafeCall(this, new MessageEventArgs(message));
            }
            finally
            {
                LastSendTime = DateTime.Now;
                TransferState = TransferState.Idle;
            }
        }

        protected void ReceiveBytes(ArraySegment<byte> bytes)
        {
            try
            {
                LastReceiveTime = DateTime.Now;
                TransferState = TransferState.Receiving;
                if (deserializeCount + bytes.Count > DeserializeBuffer.Count)//超过最大反序列化大小，直接返回
                    return;
                Buffer.BlockCopy(bytes.Array, bytes.Offset, DeserializeBuffer.Array, DeserializeBuffer.Offset, bytes.Count);
                deserializeCount += bytes.Count;
                var result = Protocol.DeSerialize(DeserializeBuffer.Array, DeserializeBuffer.Offset,deserializeCount);
                if (!result.IsNull)
                {
                    MessageReceived.SafeCall(this, new MessageEventArgs(result.Message));
                    deserializeCount -= result.ConsumeBytes;
                    Buffer.BlockCopy(DeserializeBuffer.Array, DeserializeBuffer.Offset + result.ConsumeBytes,
                        DeserializeBuffer.Array, DeserializeBuffer.Offset, result.ConsumeBytes);
                    Array.Clear(DeserializeBuffer.Array, DeserializeBuffer.Offset + deserializeCount, result.ConsumeBytes);//这一句是否需要？
                }
            }
            finally
            {
                TransferState = TransferState.Idle;
            }
        }

        protected abstract void OnSend(byte[] bytes);

        public event EventHandler<TransferStateChangedEventArgs> TransferStateChanged;

        volatile TransferState transferState = TransferState.Idle;
        public TransferState TransferState
        {
            get { return transferState; }
            private set
            {
                if (transferState == value)
                    return;
                TransferStateChanged.SafeCall(this, new TransferStateChangedEventArgs(transferState, value));
                transferState = value;
            }
        }

        public IPEndPoint RemoteEndPoint { get; protected set; }
    }
}
