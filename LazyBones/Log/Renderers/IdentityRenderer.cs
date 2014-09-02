using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 当前线程<see cref="System.Threading.Thread.CurrentPrincipal"/>的<see cref="System.Security.Principal.IIdentity"/>对象渲染器
    /// </summary>
    [Renderer("identity")]
    public class IdentityRenderer : Renderer
    {
        const char separator = ':';
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var principal = System.Threading.Thread.CurrentPrincipal;
            if (principal != null)
            {
                var identity = principal.Identity;
                if (identity != null)
                {
                    if (identity.IsAuthenticated)
                    {
                        sb.Append("已验证");
                    }
                    else
                    {
                        sb.Append("未验证");
                    }
                    sb.Append(separator);
                    sb.Append(identity.AuthenticationType);
                    sb.Append(separator);
                    sb.Append(identity.Name);
                }
            }
        }
    }
}
