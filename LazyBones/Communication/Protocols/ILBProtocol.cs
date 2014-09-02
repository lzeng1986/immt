using System.Collections.Generic;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Protocols
{
    public interface ILBProtocol
    {
        byte[] Serialize(ILBMessage message);
        DeSerializeResult DeSerialize(byte[] buffer, int offset, int count);
    }
    public struct DeSerializeResult
    {
        ILBMessage message;
        public ILBMessage Message { get { return message; } }
        int consumeBytes;
        public int ConsumeBytes { get { return consumeBytes; } }
        public DeSerializeResult(ILBMessage message, int consumeBytes)
        {
            this.message = message;
            this.consumeBytes = consumeBytes;
        }
        public bool IsNull
        {
            get { return message == null; }
        }
        public static readonly DeSerializeResult Null = new DeSerializeResult(null, 0);
    }
}
