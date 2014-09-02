using System;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 当期计算机名渲染器
    /// </summary>
    [Renderer("machineName")]
    [Static]
    public class MachineNameRenderer : Renderer
    {
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            try
            {
                sb.Append(Environment.MachineName);
            }
            catch (System.Exception ex)
            {
                TinyLog.Error("获取计算机名失败，错误:" + ex);
            }
        }
    }
}
