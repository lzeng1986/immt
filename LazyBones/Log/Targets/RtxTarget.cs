using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;
using LazyBones.Log.Layouts;

namespace LazyBones.Log.Targets
{
    [Target("rtx")]
    class RtxTarget : TargetWithLayout
    {
        IPEndPoint serverIPEP = null;
        [Required]
        public string Server
        {
            get
            {
                if (serverIPEP == null)
                    return null;
                return serverIPEP.ToString();
            }
            set
            {
                var tmp = value.Split(':');
                var ip = IPAddress.Parse(tmp[0]);
                var port = Int32.Parse(tmp[1]);
                serverIPEP = new IPEndPoint(ip, port);
            }
        }
        [DefaultValue("RTX消息")]
        public Layout Title { get; set; }
        [DefaultValue("{windowsIdentity}")]
        public Layout Sender { get; set; }

        string[] receiver;
        public string Receiver
        {
            get
            {
                if (receiver == null)
                    return null;
                return string.Join(",", receiver);
            }
            set
            {
                receiver = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        protected override void Write(LogEvent logEvent)
        {
            if (receiver == null || receiver.Length == 0)
                return;
            var sender = Sender.GetFormatMessage(logEvent);
            var title = Title.GetFormatMessage(logEvent);
            var content = Body.GetFormatMessage(logEvent); 
            foreach (var r in receiver)
            {
                SendMessage(serverIPEP, sender, r, title, content);
            }
        }

        static public void SendMessage(IPEndPoint serverIPEP, string sender, string receiver, string title, string content)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverIPEP);
                var data = Enumerable.Empty<byte>()
                    .Concat(GetBytesWithFixLength(sender, 32))
                    .Concat(GetBytesWithFixLength(receiver, 32))
                    .Concat(GetBytesWithFixLength(title, 256))
                    .Concat(GetBytesWithFixLength(content, 2048))
                    .Concat(BitConverter.GetBytes(0))
                    .Concat(GetBytes(DateTime.Now));
                var count = socket.Send(data.ToArray());
                data = null;
            }
        }
        static IEnumerable<byte> GetBytes(DateTime time)
        {
            return BitConverter.GetBytes(time.Year)
                .Concat(BitConverter.GetBytes(time.Month))
                .Concat(BitConverter.GetBytes(time.Day))
                .Concat(BitConverter.GetBytes(time.Hour));
        }
        static byte[] GetBytesWithFixLength(string str, int len)
        {
            var buffer = new byte[len];
            if (!string.IsNullOrEmpty(str))
            {
                Encoding.Default.GetBytes(str).Take(len).ToArray().CopyTo(buffer, 0);
            }
            return buffer;
        }
    }
}
