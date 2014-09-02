using System;

namespace LazyBones.Win32
{
    /// <summary>
    /// 封装钩子调用的方法
    /// </summary>
    public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
}
