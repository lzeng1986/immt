using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Protocols
{
    public class BinarySerializationProtocol : ILBProtocol
    {
        public byte[] Serialize(ILBMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, message);
                return memoryStream.ToArray();
            }
        }

        public DeSerializeResult DeSerialize(byte[] buffer, int offset, int count)
        {
            try
            {
                using (var stream = new MemoryStream(buffer, offset, count))
                {
                    var msg = (ILBMessage)new BinaryFormatter().Deserialize(stream);
                    return new DeSerializeResult(msg, (int)stream.Length);
                }
            }
            catch
            {
                return DeSerializeResult.Null;
            }
        }
    }
}
