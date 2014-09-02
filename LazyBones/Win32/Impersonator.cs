using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace LazyBones.Win32
{
    /// <summary>
    /// 提供账号模拟功能
    /// </summary>
    public class Impersonator : IDisposable
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        const int LOGON32_PROVIDER_DEFAULT = 0, LOGON32_LOGON_INTERACTIVE = 2;

        WindowsImpersonationContext wic = null;
        public Impersonator(string userName, string password, string domain)
        {
            var token = IntPtr.Zero;
            var returnValue = advapi32.LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token);
            if (!returnValue)
                throw new ArgumentException("登录失败");
            wic = WindowsIdentity.Impersonate(token);
            Kernel32.CloseHandle(token);
        }

        public void Dispose()
        {
            if (wic != null)
            {
                wic.Undo();
                wic = null;
            }   
        }

    }
}
