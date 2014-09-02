using System.Text;
using System.Threading;
using System.Globalization;

using LazyBones.Log.Config;
using LazyBones.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 当前线程Id渲染器
    /// </summary>
    [Renderer("threadId")]
    [Static]
    public class ThreadIdRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.LogThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
        }
    }
}
