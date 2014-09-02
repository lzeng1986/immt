using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 文本渲染器
    /// </summary>
    [Renderer("label")]
    public class LabelRenderer : Renderer
    {
        /// <summary>
        /// 包含的文本
        /// </summary>
        public string Text { get; set; }
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(Text);
        }
    }
}
