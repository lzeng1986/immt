using System;
using System.Text;
using LazyBones.Communication.Messages;
using LazyBones.Utils;
using System.ComponentModel;

namespace LazyBones.Communication.Protocols
{
    class LBTextProtocol<TMessage> : ILBProtocol
        where TMessage : LBTextMessage, new()
    {
        public LBTextProtocol()
            : this(Encoding.UTF8)
        {
        }
        public LBTextProtocol(Encoding encoding)
        {
            ParamGuard.NotNull(encoding, "encoding");
            Encoding = encoding;
        }

        public Encoding Encoding { get; set; }

        public byte[] GetBytes(ILBMessage message)
        {
            var msg = message as TMessage;
            if (msg == null)
                throw new ArgumentException("消息应为" + typeof(TMessage), "message");
            return Encoding.GetBytes(msg.Text);
        }

        public ILBMessage CreateMessage(byte[] data)
        {
            var msg = new TMessage { Text = Encoding.GetString(data) };
            return msg;
        }

        #region ILBProtocol Members


        public System.Collections.Generic.IEnumerable<ILBMessage> GetMessages(ArraySegment<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public int MaxSerializeLength
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int MaxDeserializeLength
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
