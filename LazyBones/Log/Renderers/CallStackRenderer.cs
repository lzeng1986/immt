using System.ComponentModel;
using System.IO;
using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>调用栈堆渲染器
    /// </summary>
    [Renderer("callStack")]
    public class CallStackRenderer : Renderer, IUseStackTrace
    {
        /// <summary>
        /// 获取或设置是否记录类名
        /// </summary>
        [DefaultValue(true)]
        public bool WithClassName { get; set; }
        /// <summary>
        /// 获取或设置是否记录方法名
        /// </summary>
        [DefaultValue(true)]
        public bool WithMethodName { get; set; }
        /// <summary>
        /// 获取或设置是否记录文件名
        /// </summary>
        [DefaultValue(true)]
        public bool WithFileName { get; set; }

        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var frame = logEvent.StackTrace != null ? logEvent.StackTrace.GetFrame(logEvent.StackFrameJumpCount) : null;
            if (frame == null)
            {
                return;
            }
            var method = frame.GetMethod();
            if (WithClassName)
            {
                if (method.DeclaringType != null)
                {
                    sb.Append(method.DeclaringType.FullName);
                }
                else
                {
                    sb.Append("<no type>");
                }
            }
            if (this.WithMethodName)
            {
                if (this.WithClassName)
                {
                    sb.Append(".");
                }
                if (method != null)
                {
                    sb.Append(method.Name);
                }
                else
                {
                    sb.Append("<no method>");
                }
            }
            if (this.WithFileName)
            {
                string fileName = frame.GetFileName();
                if (fileName != null)
                {
                    sb.AppendFormat("(source:{0} line:{1} column:{2})", Path.GetFileName(fileName), frame.GetFileLineNumber(), frame.GetFileColumnNumber());
                }
            }
        }
    }
}
