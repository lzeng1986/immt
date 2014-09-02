using System;
using System.Collections.Generic;
using System.Threading;

namespace LazyBones.Threading
{
    class ThreadEntry :IDisposable
    {
        [ThreadStatic]
        public static ThreadEntry Current;

        public bool IsWorking = false;

        public DateTime LastWorkTime { get; private set; }

        public DateTime CreateTime { get; private set; }

        public LinkedListNode<ThreadEntry> WaitEntry = null;

        public AutoResetEvent WaitHandle = new AutoResetEvent(false);

        public ThreadJob Job;   // 表示ThreadEntry的当前作业 [11/20/2013 zliang]

        public void Refresh()
        {
            LastWorkTime = DateTime.Now;
        }

        public ThreadEntry(Thread thread)
        {
            CreateTime = DateTime.Now;
            LastWorkTime = DateTime.MinValue;
        }
        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                WaitHandle.Close();
                WaitHandle = null;
                WaitEntry = null;
                isDisposed = true;
            }
        }
    }
}
