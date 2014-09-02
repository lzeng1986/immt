using System.ComponentModel;
using System.Security.Principal;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 当前<see cref="WindowsIdentity"/>对象渲染器
    /// </summary>
    [Renderer("windowsIdentity")]
    public class WindowsIdentityRenderer : Renderer
    {
        [DefaultValue(true)]
        public bool WithDomain { get; set; }

        [DefaultValue('\\')]
        [Required]
        public char Separator { get; set; }

        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var wid = WindowsIdentity.GetCurrent();
            if (wid == null)
                return;
            if (WithDomain)
            {
                sb.Append(wid.Name.Replace('\\', Separator));
            }
            else
            {
                var ind = wid.Name.LastIndexOf('\\');
                sb.Append(wid.Name.Substring(ind + 1));
            }
        }
    }
}
