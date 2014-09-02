using System;
using System.Collections.Generic;
using System.Threading;
using LazyBones.Communication.Config;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;
using LazyBones.Threading;

namespace LazyBones.Communication.Channels
{
    /// <summary>
    /// 实现一个“请求-等待回复”模型的通道
    /// </summary>
    /// <typeparam name="TChannel"></typeparam>
    public class RequestResponseChannel<TChannel> : Communicator, ILBChannel
        where TChannel : ILBChannel
    {
        ILBChannel innerChannel;
        Dictionary<string, MessageWaitHandle> messageWaitHandles = new Dictionary<string, MessageWaitHandle>();
        BackgroundQueueWorker sendMessageWorker = new BackgroundQueueWorker(1024);
        object syncObj = new object();
        public RequestResponseChannel(ILBChannel innerChannel)
        {
            this.innerChannel = innerChannel;
            WaitResponseTimeOut = Defaults.WaitResponseTimeOut;
        }

        protected override void OpenCommunicator()
        {
            innerChannel.MessageReceived += innerChannel_MessageReceived;
            innerChannel.MessageSent += innerChannel_MessageSent;
            innerChannel.Open();
        }

        protected override void CloseCommunicator()
        {
            sendMessageWorker.Dispose();
            innerChannel.Close();
            innerChannel.MessageReceived -= innerChannel_MessageReceived;
            innerChannel.MessageSent -= innerChannel_MessageSent;
            innerChannel = null;
            lock (syncObj)
            {
                foreach (var handle in messageWaitHandles.Values)
                {
                    handle.Result = WaitResult.Canceled;
                    handle.WaitHandle.Set();
                }
                messageWaitHandles.Clear();
            }
        }
        
        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<MessageEventArgs> MessageSent;

        public void SendMessage(ILBMessage message)
        {
            innerChannel.SendMessage(message);
        }

        public ILBMessage SendMessageAndWaitResponse(ILBMessage message)
        {
            return SendMessageAndWaitResponse(message, WaitResponseTimeOut);
        }

        public ILBMessage SendMessageAndWaitResponse(ILBMessage message, int timeout)
        {
            if (string.IsNullOrEmpty(message.Id))
                throw new ArgumentNullException("message", "消息的Id为空");
            var handle = new MessageWaitHandle();
            lock (syncObj)
            {
                messageWaitHandles[message.Id] = handle;
            }
            try
            {
                SendMessage(message);
                handle.WaitHandle.WaitOne(timeout);
                switch (handle.Result)
                {
                    case WaitResult.Timeout:
                        throw new TimeoutException("在规定时间内没有收到回复消息");
                    case WaitResult.Canceled:
                        throw new CommunicationException("操作被取消");
                    default:
                        return handle.Response;
                }
            }
            finally
            {
                lock (syncObj)
                {
                    messageWaitHandles.Remove(message.Id);
                }
            }
        }

        protected virtual void innerChannel_MessageSent(object sender, MessageEventArgs e)
        {
            OnMessageSent(e.Message);
        }
        protected virtual void OnMessageSent(ILBMessage message)
        {
            var handler = MessageSent;
            if (handler != null)
                handler(this, new MessageEventArgs(message));
        }
        void innerChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message.ReplyId))
            {
                MessageWaitHandle handle;
                lock (syncObj)
                {
                    messageWaitHandles.TryGetValue(e.Message.ReplyId, out handle);
                }
                if (handle != null)
                {
                    handle.Response = e.Message;
                    handle.Result = WaitResult.OK;
                    handle.WaitHandle.Set();
                }
            }
            else
            {
                sendMessageWorker.RunAsync(OnMessageSent, e.Message);
            }
        }
        public LBProtocol Protocol
        {
            get { return innerChannel.Protocol; }
        }

        void OnMessageReceived(object sender, MessageEventArgs args)
        {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, args);
        }

        public int WaitResponseTimeOut { get; set; }

        public TransferState TransferState
        {
            get { return innerChannel.TransferState; }
        }

        public System.Net.IPEndPoint RemoteEndPoint
        {
            get { return innerChannel.RemoteEndPoint; }
        }

        class MessageWaitHandle
        {
            public ILBMessage Response;
            public ManualResetEvent WaitHandle = new ManualResetEvent(false);
            public WaitResult Result = WaitResult.Timeout;
        }

        enum WaitResult
        {
            OK,
            Canceled,
            Timeout
        }
    }
}
