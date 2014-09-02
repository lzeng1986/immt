using System;
using LazyBones.Extensions;

namespace LazyBones.Win32
{
    /// <summary>
    /// 表示一个当前线程的Windows钩子
    /// </summary>
    public class LocalWinHook: IDisposable
    {
        IntPtr hookHandle = IntPtr.Zero;
        HookType hookType;

        /// <summary>
        /// 在钩子被调用时发生
        /// </summary>
        public event EventHandler<HookEventArgs> HookInvoked;
        protected void OnHookInvoked(HookEventArgs e)
        {
            HookInvoked.SafeCall(this, e);
        }

        public LocalWinHook(HookType hook)
        {
            hookType = hook;
        }

        IntPtr CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return User32.CallNextHookEx(hookHandle, code, wParam, lParam);
            OnHookInvoked(new HookEventArgs(code, wParam, lParam));
            return User32.CallNextHookEx(hookHandle, code, wParam, lParam);
        }
        /// <summary>
        /// 为当前线程安装钩子
        /// </summary>
        public void Install()
        {
            if (hookHandle != IntPtr.Zero)
                Uninstall();

            var threadId = Kernel32.GetCurrentThreadId();

            hookHandle = User32.SetWindowsHookEx(hookType, CoreHookProc, IntPtr.Zero, threadId);
        }
        /// <summary>
        /// 卸载钩子
        /// </summary>
        public void Uninstall()
        {
            if (hookHandle != IntPtr.Zero)
            {
                User32.UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Uninstall();
        }
    }
    public class HookEventArgs : EventArgs
    {
        public int HookCode { get; private set; }
        public IntPtr WParam { get; private set; }
        public IntPtr LParam { get; private set; }
        public HookEventArgs(int hookCode, IntPtr wParam, IntPtr lParam)
        {
            HookCode = hookCode;
            WParam = wParam;
            LParam = lParam;
        }
    }
}
