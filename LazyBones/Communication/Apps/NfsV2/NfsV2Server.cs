using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Communication.Apps.NfsV2
{
    public class NfsV2Server : IAppServer
    {
        public LBBinding Binding
        {
            get { return new NfsV2Binding(); }
        }

        public string Name
        {
            get { return "NfsV2 server"; }
        }

        List<MountExporter> exports;

        public List<MountExporter> Exports
        {
            get { return exports; }
        }


        public string GetNewClientId(System.Net.IPEndPoint remoteIPEP)
        {
            throw new NotImplementedException();
        }
    }

    public class ExportItem
    {
    }
}
