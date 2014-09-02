using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.TimeStamp"/>属性日期部分渲染器，格式"yyyy-MM-dd"
    /// </summary>
    [Renderer("date")]
    [Static]
    public class DateRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var time = logEvent.TimeStamp;
            sb.Append(time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}
