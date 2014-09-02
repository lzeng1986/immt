using System;
using System.Linq;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Apps.NfsV2
{
    public class RpcV2Protocol : ILBProtocol
    {
        public byte[] Serialize(ILBMessage message)
        {
            var msg = message as RpcPacket;
            return msg.RawData;
        }

        public DeSerializeResult DeSerialize(byte[] buffer, int offset, int count)
        {
            try
            {
                var msg = new RpcPacket(buffer, offset, count);
                return new DeSerializeResult(msg, count);
            }
            catch
            {
                return DeSerializeResult.Null;
            }
        }
    }
}
