using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using LazyBones.Config;

namespace LazyBones.Communication.Config
{
    [Serializable]
    [XmlRoot(Namespace = "LazyBones", ElementName = "server")]
    [Element("server")]
    public class ServerConfig
    {
        [Item("name", Required = true)]
        public string Name { get; set; }

        [Item("mode", Default = Defaults.Mode)]
        public SocketMode Mode { get; set; }

        [Item("sendTimeOut", Default = Defaults.SendTimeOut)]
        public int SendTimeOut { get; set; }

        [Item("reveiveTimeOut", Default = Defaults.ReceiveTimeOut)]
        public int ReveiveTimeOut { get; set; }

        [Item("reveiveTimeOut", Default = Defaults.ReceiveTimeOut)]
        public int ConnectionTimeOut { get; set; }

        [Item("syncSend", Default = false)]
        public bool SyncSend { get; set; }

        [Item("idleSessionTimeOut", Default = Defaults.IdleSessionTimeOut)]
        public int IdleSessionTimeOut { get; set; }

        [XmlAttribute(AttributeName = "keepAliveTime", Type = typeof(int))]
        [DefaultValue(10000)]
        public int KeepAliveTime { get; set; }

        [Item("maxConnection", Default = Defaults.MaxConnection)]
        public int MaxConnection { get; set; }

        [Item("receiveBufferSize", Default = Defaults.ReceiveBufferSize)]
        public int ReceiveBufferSize { get; set; }

        [Item("sendBufferSize", Default = Defaults.SendBufferSize)]
        public int SendBufferSize { get; set; }

        [Item("certificate", Default = null)]
        public X509Certificate Certificate { get; set; }

        [Item("CredentialType", Default = CredentialType.None)]
        public CredentialType CredentialType { get; set; }

        public int BackLog { get; set; }

        public int MaxRequestLength { get; set; }

        [Item("uri", Required = true)]
        public Uri Uri { get; set; }
    }
}
