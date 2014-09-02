using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using LazyBones.Log.Config;
using LazyBones.Extensions;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 垃圾回收器<see cref="GC"/>的渲染器
    /// </summary>
    [Renderer("gc")]
    public class GCRenderer : Renderer
    {
        /// <summary>
        /// 渲染器记录的对象
        /// </summary>
        [DefaultValue(GCItem.TotalMemory)]
        public GCItem Item { get; set; }
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            if (Item.HasFlag(GCItem.TotalMemory))
            {
                sb.Append("TotalMemory:");
                sb.Append(GC.GetTotalMemory(false).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            if (Item.HasFlag(GCItem.TotalMemoryForceCollection))
            {
                sb.Append("TotalMemoryForceCollection:");
                sb.Append(GC.GetTotalMemory(true).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            if (Item.HasFlag(GCItem.Gen0))
            {
                sb.Append("Gen0:");
                sb.Append(GC.CollectionCount(0).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            if (Item.HasFlag(GCItem.Gen1))
            {
                sb.Append("Gen1:");
                sb.Append(GC.CollectionCount(1).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            if (Item.HasFlag(GCItem.Gen2))
            {
                sb.Append("Gen2:");
                sb.Append(GC.CollectionCount(2).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            if (Item.HasFlag(GCItem.MaxGen))
            {
                sb.Append("MaxGen:");
                sb.Append(GC.MaxGeneration.ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
        }
    }
}
