using System;

namespace LazyBones.Win32
{
    /// <summary>
    /// 提供WinApi的封装
    /// </summary>
    [Author("曾樑")]
    public static class Native
    {
        /// <summary>
        /// 确定当前Windows版本为Windows 2000或更高版本
        /// </summary>
        public static readonly bool IsWin2KOrLater = (Environment.OSVersion.Version.Major >= 5);

        ///// <summary>
        ///// 获取系统范围内键盘输入焦点，如果只想获取当前线程输入焦点，应使用 User32.GetFocus
        ///// </summary>
        ///// <returns></returns>
        //public static IntPtr GetFocus()
        //{
        //    var fgWinHandle = User32.GetForegroundWindow();
        //    var winThread = User32.GetWindowThreadProcessId(fgWinHandle, IntPtr.Zero);
        //    var currentThreadId = Kernel32.GetCurrentThreadId();
        //    User32.AttachThreadInput(winThread, currentThreadId, true);
        //    var handle = User32.GetFocus();
        //    User32.AttachThreadInput(winThread, currentThreadId, false);
        //    return handle;
        //}
    }
}
