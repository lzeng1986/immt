using System.Collections.Generic;

namespace LazyBones.Threading
{
    // 用于线程池内部处理作业序列 [10/23/2013 zliang]
    class ProcessingQueue
    {
        LinkedList<ThreadJob> workJobQueue = new LinkedList<ThreadJob>();
        LinkedList<ThreadEntry> waitHandleList = new LinkedList<ThreadEntry>();

        public int Count
        {
            get
            {
                return workJobQueue.Count;
            }
        }

        public void Dequeue(int timeout)
        {
            lock (this)
            {
                while (workJobQueue.Count > 0)
                {
                    var job = workJobQueue.First.Value;
                    workJobQueue.RemoveFirst();
                    if (job.IsCanceled)
                    {
                        continue;
                    }
                    ThreadEntry.Current.Job = job;
                    return;
                }
                if (ThreadEntry.Current.WaitEntry == null)
                {
                    ThreadEntry.Current.WaitEntry = waitHandleList.AddFirst(ThreadEntry.Current);
                }
                else
                {
                    if (ThreadEntry.Current.WaitEntry.List != null)
                        waitHandleList.Remove(ThreadEntry.Current.WaitEntry);
                    waitHandleList.AddFirst(ThreadEntry.Current.WaitEntry);
                }
            }
            if (ThreadEntry.Current.WaitHandle.WaitOne(timeout))
            {
                lock (this)
                {
                    if (ThreadEntry.Current.WaitEntry.List != null)
                        waitHandleList.Remove(ThreadEntry.Current.WaitEntry);
                    if (ThreadEntry.Current.Job == null)
                    {
                        ThreadEntry.Current.Job = workJobQueue.First.Value;
                        workJobQueue.RemoveFirst();
                    }
                }
            }
        }
        public void Remove(ThreadEntry threadEntry)
        {
            lock (this)
            {
                if (threadEntry.WaitEntry.List != null)
                {
                    waitHandleList.Remove(threadEntry.WaitEntry);
                }
            }
        }
        public void Enqueue(ThreadJob job)
        {
            lock (this)
            {
                if (waitHandleList.Count > 0)   //检查是否有等在线程，如果有则直接将作业发送给等待线程
                {
                    var currentEntry = waitHandleList.First.Value;
                    waitHandleList.RemoveFirst();
                    currentEntry.Job = job;
                    System.Diagnostics.Debug.Assert(currentEntry.WaitHandle != null);
                    currentEntry.WaitHandle.Set();
                }
                else
                {
                    workJobQueue.AddLast(job);
                    job.IsQueued();
                }
            }
        }
    }


}
