
namespace LazyBones.Win32
{
    /// <summary>
    /// 用于传递窗体闪烁参数
    /// </summary>
    public struct FlashWInfo
    {
        public uint Size;
        public System.IntPtr Handle;
        public FlashFlag Flags;
        public uint Count;
        public int Timeout;
    }
    /// <summary>
    /// 表示窗体闪烁方式
    /// </summary>
    [System.Flags]
    public enum FlashFlag : uint
    {
        /// <summary>
        /// 同时闪烁窗体标题和任务栏
        /// </summary>
        All = Caption | Tray,
        /// <summary>
        /// 停止闪烁
        /// </summary>
        Stop = 0x00,
        /// <summary>
        /// 闪烁窗体标题
        /// </summary>
        Caption = 0x01,
        /// <summary>
        /// 闪烁窗体任务栏
        /// </summary>
        Tray = 0x02,
        /// <summary>
        /// 一直闪烁
        /// </summary>
        Timer = 0x04,
        /// <summary>
        /// 一直闪烁至窗体激活
        /// </summary>
        TimerNoForeground = 0x0C
    }
}
