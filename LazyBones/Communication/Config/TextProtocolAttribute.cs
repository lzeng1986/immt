using System;
using System.Text;

namespace LazyBones.Communication.Config
{
    public class ServerAttribute : Attribute
    {
        public string Schema { get; private set; }
        public SocketMode SocketMode { get; set; }
        public ServerAttribute(string schema)
        {
            if (string.IsNullOrEmpty(schema))
                throw new ArgumentNullException("schema");
            Schema = schema;
            SocketMode = SocketMode.Tcp;
        }
    }
    public class ClientAttribute : Attribute
    {
        public string Schema { get; private set; }
        public SocketMode SocketMode { get; set; }
        public ClientAttribute(string schema)
        {
            if (string.IsNullOrEmpty(schema))
                throw new ArgumentNullException("schema");
            Schema = schema;
            SocketMode = SocketMode.Tcp;
        }
    }
    public enum RunMode
    {
        Server,
        Client
    }
}
