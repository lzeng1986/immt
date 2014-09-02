using System.IO;
using System.Text;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpProtocol : ILBProtocol
    {
        public FtpProtocol()
        {
            UsedEncoding = Encoding.Default;
        }
        public Encoding UsedEncoding { get; set; }

        public byte[] Serialize(ILBMessage message)
        {
            var msg = message as FtpMessage;
            if (msg.Text.EndsWith("\r\n"))
                return UsedEncoding.GetBytes(msg.Text);
            else
                return UsedEncoding.GetBytes(msg.Text + "\r\n");
        }

        public DeSerializeResult DeSerialize(byte[] buffer, int offset, int count)
        {
            using (var stream = new MemoryStream(buffer, offset, count))
            using (var reader = new StreamReader(stream, UsedEncoding))
            {
                var sb = new StringBuilder();
                while (true)
                {
                    var c = reader.Read();
                    if (c == -1)
                        break;
                    if (c == '\r')
                    {
                        var next = stream.ReadByte();
                        if (next == '\n')
                            break;
                    }
                    sb.Append((char)c);
                }
                if (sb.Length == 0)
                    return DeSerializeResult.Null;
                else
                    return new DeSerializeResult(new FtpMessage { Text = sb.ToString() }, (int)stream.Position);
            }
        }
    }
}
