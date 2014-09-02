using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.TimeStamp"/>属性长日期格式("yyyy-MM-dd HH:mm:ss.fff")渲染器
    /// </summary>
    [Renderer("longDateTime")]
    [Static]
    public class LongDateTimeRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var time = logEvent.TimeStamp;
            sb.Append(time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
        }
    }
}
