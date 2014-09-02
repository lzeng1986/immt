using System;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 渲染器<see cref="GCRenderer"/>记录对象的枚举值
    /// </summary>
    [Flags]
    public enum GCItem
    {
        /// <summary>
        /// 获取分配的总字节数
        /// </summary>
        TotalMemory = 0x01,
        /// <summary>
        /// 获取分配的总字节数，在获取之前如果正在进行垃圾回收，则等待垃圾回收完成
        /// </summary>
        TotalMemoryForceCollection = 0x02,
        /// <summary>
        /// 获取第0代对象回收次数
        /// </summary>
        Gen0 = 0x04,
        /// <summary>
        /// 获取第1代对象回收次数
        /// </summary>
        Gen1 = 0x08,
        /// <summary>
        /// 获取第2代对象回收次数
        /// </summary>
        Gen2 = 0x10,
        /// <summary>
        /// 获取系统当前支持的最大代数
        /// </summary>
        MaxGen = 0x20
    }
}
