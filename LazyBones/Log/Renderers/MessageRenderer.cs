using System.ComponentModel;
using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 日志对象<see cref="LogEvent"/>包含消息的渲染器
    /// </summary>
    [Renderer("message")]
    public class MessageRenderer : Renderer
    {
        /// <summary>
        /// 获取或设置是否记录错误信息
        /// </summary>
        [DefaultValue(false)]
        public bool LogException { get; set; }

        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.Message);
            if (LogException && logEvent.Exception != null)
            {
                sb.Append(" detail:");
                sb.Append(logEvent.Exception.ToString());
            }
        }
    }
}
