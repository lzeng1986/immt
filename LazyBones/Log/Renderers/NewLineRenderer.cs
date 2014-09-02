using System;
using System.Text;

using LazyBones.Log.Config;
using LazyBones.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 换行字符串渲染器，此渲染器使用<see cref="Environment.NewLine"/>
    /// </summary>
    [Renderer("newline")]
    [Static]
    public class NewLineRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(Environment.NewLine);
        }
        
    }
}
