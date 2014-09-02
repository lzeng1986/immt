using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.LogEventId"/>属性渲染器
    /// </summary>
    [Renderer("logEventId")]
    [Static]
    public class IdRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.LogEventId.ToString(CultureInfo.InvariantCulture));
        }
    }
}
