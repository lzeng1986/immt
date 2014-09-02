using System.Xml.Serialization;
using System.ComponentModel;

namespace LazyBones.Communication.Apps.Ftp
{
    [XmlRoot("ftp")]
    public class FtpConfig
    {
        [XmlElement("autoReload"), DefaultValue(true)]
        public bool AutoReload { get; set; }
        [XmlElement("pasvMinPort")]
        public int PasvMinPort { get; set; }
        [XmlElement("pasvMaxPort")]
        public int PasvMaxPort { get; set; }
        [XmlArray("policyList")]
        [XmlArrayItem("item", Type = typeof(FtpPolicy), IsNullable = true)]
        public FtpPolicy[] Policies { get; set; }
    }
}
