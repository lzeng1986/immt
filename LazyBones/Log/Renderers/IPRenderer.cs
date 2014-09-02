using System;
using System.Net;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    [Renderer("ip")]
    [Static]
    public class IPRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            try
            {
                foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    sb.AppendLine(ip.ToString());
                }
            }
            catch (Exception ex)
            {
                TinyLog.Error(ex.Message);
            }
        }
    }
}
