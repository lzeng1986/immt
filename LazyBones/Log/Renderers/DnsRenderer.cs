using System.ComponentModel;
using System.Net;
using System.Text;
using LazyBones.Log.Config;
using System.Net.Sockets;

namespace LazyBones.Log.Renderers
{
    [Renderer("dns")]
    class DnsRenderer : Renderer
    {
        [DefaultValue(false)]
        public bool IncludeHostName { get; set; }

        [DefaultValue(false)]
        public bool IncludeIPv6 { get; set; }

        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            if (IncludeHostName)
            {
                sb.Append(ipHostEntry.HostName);
                sb.Append(' ');
            }
            foreach (var ip in ipHostEntry.AddressList)
            {
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        sb.Append("IPv4地址：");
                        sb.Append(ip);
                        sb.Append(' ');
                        break;
                    case AddressFamily.InterNetworkV6:
                        if (IncludeIPv6)
                        {
                            sb.Append("IPv6地址：");
                            sb.Append(ip);
                            sb.Append(' ');
                        }
                        break;
                }
            }
        }
    }
}
