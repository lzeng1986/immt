using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 当前线程Name渲染器
    /// </summary>
    [Renderer("threadName")]
    [Static]
    public class ThreadNameRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var name = logEvent.LogThread.Name;
            if (string.IsNullOrEmpty(name))
                return;
            sb.Append(name);
        }
    }
}
