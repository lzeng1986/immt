using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.TimeStamp"/>属性Ticks渲染器
    /// </summary>
    [Renderer("ticks")]
    [Static]
    public class TicksRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.TimeStamp.Ticks.ToString(CultureInfo.InvariantCulture));
        }
    }
}
