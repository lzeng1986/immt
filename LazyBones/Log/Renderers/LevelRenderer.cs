using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>的<see cref="LogEvent.Level"/>属性渲染器
    /// </summary>
    [Renderer("level")]
    [Static]
    public class LevelRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.Level.ToString());
        }
    }
}
