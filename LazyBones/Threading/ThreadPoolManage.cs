using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LazyBones.Threading
{
    /// <summary>
    /// 提供类级别的线程池访问，也可自定义线程池
    /// </summary>
    public static class ThreadPoolManage
    {
        static LBThreadPool defaultPool;
        static Dictionary<int, LBThreadPool> threadPoolFactory = new Dictionary<int, LBThreadPool>();
        static ThreadPoolManage()
        {
            SetTerminationEvents();
        }
        static void SetTerminationEvents()
        {
            AppDomain.CurrentDomain.DomainUnload += CloseThreadPool;
            AppDomain.CurrentDomain.ProcessExit += CloseThreadPool;
        }
        static void CloseThreadPool(object sender, EventArgs args)
        {
            defaultPool.Close();
        }
        public static LBThreadPool DefaultPool
        {
            get
            {
                return defaultPool ?? (defaultPool = new LBThreadPool() { Name = "#DefaultThreadPool" });
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static LBThreadPool GetCurrentPool()
        {
            return null;
        }
        public static LBThreadPool GetThreadPool(string name)
        {
            return null;
        }
    }
}
