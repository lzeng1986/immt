using System;
using System.ComponentModel;
using System.Net;
using System.Security.Authentication;
using System.Xml.Serialization;
using System.Text;
using LazyBones.Communication.Security;

namespace LazyBones.Communication.Config
{
    [Serializable]
    [XmlRoot(Namespace = "LazyBones", ElementName = "listener")]
    public class ListenerConfig
    {
        [XmlAttribute(AttributeName = "address")]
        public string Address { get; set; }

        [XmlAttribute(AttributeName = "port")]
        public string Port { get; set; }

        [XmlAttribute(AttributeName = "backlog")]
        [DefaultValue(0)]
        public int Backlog { get; set; }

        [XmlAttribute(AttributeName = "security")]
        public ISecurityBinding SecurityBinding { get; set; }
    }
    public enum CredentialType
    {
        None,
        Ssl2 = 12,
        Ssl3 = 48,
        Tls = 192,
        Default = 240,
        Negotiate
    }
}
