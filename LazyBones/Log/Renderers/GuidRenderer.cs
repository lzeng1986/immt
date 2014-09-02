using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// <see cref="System.Guid"/>对象渲染器
    /// </summary>
    [Renderer("guid")]
    public class GuidRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(System.Guid.NewGuid().ToString());
        }
    }
}
