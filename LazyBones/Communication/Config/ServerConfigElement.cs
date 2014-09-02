using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LazyBones.Config;

namespace LazyBones.Communication.Config
{
    class ServerConfigElement
    {
        readonly XElement source;
        Dictionary<string, string> attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public ServerConfigElement(XElement source)
        {
            this.source = source;
        }
        public string Name { get; private set; }
        public Dictionary<string, string> Attributes { get { return attributes; } }
    }
}
