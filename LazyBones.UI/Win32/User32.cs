using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace LazyBones.Win32
{
    /// <summary>
    /// 提供对User32.dll的访问
    /// </summary>
    public static class User32
    {
        [DllImport("User32.dll")]
        public static extern bool FlashWindowEx(ref FlashWInfo info);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DragDetect(IntPtr hWnd, Point pt);

        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("User32.dll")]
        public static extern bool GetLastInputInfo(ref LastInputInfo info);

        [DllImport("User32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        public static extern bool AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out int processId);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr id);

        //----------针对SendMessage的各种重载------------------------
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, WinMsg msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, WinMsg msg, int wParam, string lParam);
        //--------------------------------------------------------       

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(Win32.HookType code, HookProc func, IntPtr hInstance, IntPtr threadID);

        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, SetWindowPosFlags flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowLong(IntPtr hWnd, GetWindowLongIndex Index);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);
    }
    
}
