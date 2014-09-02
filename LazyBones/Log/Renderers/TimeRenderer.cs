using System.ComponentModel;
using System.Globalization;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.TimeStamp"/>属性时间部分渲染器，格式"HH:mm:ss"
    /// </summary>
    [Renderer("time")]
    [Static]
    public class TimeRenderer : Renderer
    {
        [DefaultValue(false)]
        public bool IncludeMillisecond { get; set; }
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var time = logEvent.TimeStamp;
            if (IncludeMillisecond)
                sb.Append(time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            else
                sb.Append(time.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
