using System;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 与当前用户关联的网络域名
    /// </summary>
    [Renderer("domainName")]
    [Static]
    public class DomainNameRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            try
            {
                sb.Append(Environment.UserDomainName);
            }
            catch (System.Exception ex)
            {
                TinyLog.Error("Get domainName failed,error" + ex);
            }
        }
    }
}
