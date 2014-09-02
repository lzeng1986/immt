using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.TimeStamp"/>属性短日期格式("yyyy-MM-dd HH:mm")渲染器
    /// </summary>
    [Renderer("shortDateTime")]
    [Static]
    public class ShortDateTimeRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var time = logEvent.TimeStamp;
            sb.Append(time.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
        }
    }
}
