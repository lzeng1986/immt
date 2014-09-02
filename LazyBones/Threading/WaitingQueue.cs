using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LazyBones.Threading
{
    // 用于线程池内部处理作业序列 [10/23/2013 zliang]
    class WaitingQueue : IDisposable
    {
        PriorityQueue taskQueue = new PriorityQueue();
        LinkedList<WaitEntry> waitEntryList = new LinkedList<WaitEntry>();
        object locker = new object();

        bool isDisposed = false;

        public int Count
        {
            get { return taskQueue.Count; }
        }

        public Task Dequeue(int timeout, WaitHandle closeHandle)
        {
            WaitEntry waitEntry;
            //从队列中获取一个任务，如果有等待的任务，则直接获取，如果没有，则等待任务
            //等待任务返回有三种情况：1.有新任务到来，成功返回；2.等待timeout超时，返回null；3.线程池关闭，返回null
            lock (locker)
            {
                CheckDispose();
                if (taskQueue.Count > 0)
                {
                    return taskQueue.Dequeue();//有等待的任务，直接返回
                }
                waitEntry = WaitEntry.Current ?? (WaitEntry.Current = new WaitEntry());
                waitEntry.Reset();
                //将当前ThreadEntry放在等待队列首部，如果当前ThreadEntry已存在于等待队列中，则将其移至队列首部
                if (waitEntry.LinkNode.List != null)
                    waitEntryList.Remove(waitEntry.LinkNode);
                waitEntryList.AddFirst(waitEntry.LinkNode);
            }
            var result = WaitHandle.WaitAny(new[] { waitEntry.WaitHandle, closeHandle }, timeout, true);
            lock (locker)
            {
                if (result == 1)//情况3.线程池关闭，返回null
                {
                    return null;
                }
                else
                {
                    if (result == WaitHandle.WaitTimeout && waitEntry.IsTimeout)//情况2.等待timeout超时，返回null
                    {
                        waitEntryList.Remove(waitEntry.LinkNode);
                        return null;
                    }
                    //情况1.有新任务到来，成功返回
                    return waitEntry.Task ?? taskQueue.Dequeue();
                }
            }
        }
        public void Enqueue(Task task)
        {
            Debug.Assert(task != null);

            lock (locker)
            {
                CheckDispose();
                //检查是否有等在线程，如果有则直接将作业发送给等待线程，否则添加到队列中等待执行
                while (waitEntryList.Count > 0)
                {
                    var entry = waitEntryList.First.Value;
                    waitEntryList.RemoveFirst();
                    if (entry.SetTask(task))
                    {
                        return;
                    }
                }
                taskQueue.Enqueue(task);
                task.Queued();
            }
        }


        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                taskQueue.Clear();

                foreach (var entry in waitEntryList)
                    entry.Dispose();
                waitEntryList.Clear();
            }
        }

        void CheckDispose()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        class WaitEntry : IDisposable
        {
            [ThreadStatic]
            public static WaitEntry Current;

            public LinkedListNode<WaitEntry> LinkNode { get; private set; }

            AutoResetEvent waitHandle = new AutoResetEvent(false);
            public WaitHandle WaitHandle
            {
                get { return waitHandle; }
            }

            public Task task;

            public Task Task// 表示当前作业 [11/20/2013 zliang]
            {
                get { return task; }
            }
            public WaitEntry()
            {
                LinkNode = new LinkedListNode<WaitEntry>(this);
            }
            bool isDisposed = false;
            public void Dispose()
            {
                lock (this)
                {
                    if (!isDisposed)
                    {
                        waitHandle.Close();
                        task = null;
                        LinkNode = null;
                        isDisposed = true;
                    }
                }
            }
            bool isTimeout = false;
            public bool IsTimeout
            {
                get
                {
                    lock (this)
                    {
                        if (Task == null)
                        {
                            isTimeout = true;
                        }
                        return isTimeout;
                    }
                }
            }
            public bool SetTask(Task task)
            {
                lock (this)
                {
                    if (isTimeout || isDisposed)
                        return false;
                    this.task = task;
                    waitHandle.Set();
                    return true;
                }
            }
            public void Reset()
            {
                task = null;
                isTimeout = false;
                waitHandle.Reset();
            }
        }
    }
}
