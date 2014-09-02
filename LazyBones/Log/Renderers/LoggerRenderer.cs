using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    [Renderer("logger")]
    class LoggerRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            sb.Append(logEvent.Logger.Name);
        }
        //[Default]
        public bool IncludeLogLevel { get; set; }
    }
}
