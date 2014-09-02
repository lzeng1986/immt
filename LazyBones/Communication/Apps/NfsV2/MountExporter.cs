using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LazyBones.Communication.Apps.NfsV2
{
    [XmlRoot("exporter")]
    public class MountExporter
    {
        [XmlAttribute("name")]
        public string ExportName { get; set; }
        [XmlAttribute("path")]
        public string Path { get; set; }
        [XmlAttribute("allowedIP")]
        public string AllowedIP { get; set; }
    }

}
