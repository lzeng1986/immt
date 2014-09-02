using System.Xml.Serialization;

namespace LazyBones.Communication.Security
{
    public interface ISecurityBinding
    {
        [XmlAttribute(AttributeName = "type")]
        CredentialType CredentialType { get; }
    }
}
