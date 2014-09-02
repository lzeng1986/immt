using System;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    [Renderer("environment")]
    [Static]
    public class EnvironmentRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            try
            {
                sb.AppendLine("MachineName:" + Environment.MachineName);
            }
            catch{}
            try
            {
                sb.AppendLine("OSVersion:" + Environment.OSVersion);
            }
            catch { }
            sb.AppendLine("DoNetVersion:" + Environment.Version);
            sb.AppendLine("ProcessorCount:" + Environment.ProcessorCount);
        }
    }
}
