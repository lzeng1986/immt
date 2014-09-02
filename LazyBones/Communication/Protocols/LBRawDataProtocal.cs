using System;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Protocols
{
    public class LBRawDataProtocal<TMessage> : LBProtocol
        where TMessage : LBRawDataMessage, new()
    {
        public byte[] GetBytes(ILBMessage message)
        {
            var msg = message as TMessage;
            if (msg == null)
                throw new ArgumentException("消息应为" + typeof(TMessage), "message");
            return msg.RawData;
        }

        public ILBMessage CreateMessage(byte[] data)
        {
            var msg = new TMessage() { RawData = data};
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
