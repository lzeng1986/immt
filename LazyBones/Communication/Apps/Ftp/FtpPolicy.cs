using System.ComponentModel;
using System.Xml.Serialization;

namespace LazyBones.Communication.Apps.Ftp
{
    //ftp用户策略
    [XmlRoot("policy")]
    public class FtpPolicy
    {
        [XmlAttribute("root")]
        public string Root { get; set; }
        [XmlAttribute("user")]
        public string User { get; set; }
        [XmlAttribute("password"), DefaultValue("")]
        public string Password { get; set; }
        [XmlAttribute("allowedIP"), DefaultValue("")]
        public string AllowedIP { get; set; }
        [XmlAttribute("refusedIP"), DefaultValue("")]
        public string RefusedIP { get; set; }
        [XmlAttribute("permission"), DefaultValue(UserPermissions.Default)]
        public UserPermissions Permission { get; set; }
    }
}
