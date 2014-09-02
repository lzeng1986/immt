using System;
using System.Collections.Generic;
using System.Threading;

namespace LazyBones.Threading
{
    public class TaskQueue : TaskHandle
    {
        LBThreadPool threadPool;
        PriorityQueue taskQueue = new PriorityQueue();
        object syncObj = new object();
        int taskExecuting = 0;
        ManualResetEvent waitAnyHandle = new ManualResetEvent(true);

        static long SequenceNo = 0;

        internal TaskQueue(LBThreadPool threadPool)
        {
            this.threadPool = threadPool;
            Name = threadPool.Name + '#' + Interlocked.Increment(ref SequenceNo);
        }

        public override bool WaitForIdle(int millisecondsTimeout)
        {
            LBThreadPool.ValidateTaskHandleWaitForIdle(this);
            return base.WaitForIdle(millisecondsTimeout);
        }

        int concurrency = Environment.ProcessorCount;//初始值为机器处理器的数量
        public override int Concurrency
        {
            get { return concurrency; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Concurrency", "Concurrency不得小于0");
                var diff = value - concurrency;
                concurrency = value;
                for (int i = 0; i < diff; ++i)
                {
                    ProcessNextTask();
                }
            }
        }

        protected override void CloseInternal(bool forceAbort, int timeout)
        {
            lock (syncObj)
            {
                foreach (var task in taskQueue)
                    task.Dispose();
                taskQueue.Clear();
                if (WaitForIdle(timeout))
                    return;
                threadPool.RemoveParallel(this);
                if (forceAbort)
                { 
                    //不做任何处理
                }
            }
        }

        public override void RunAsync(Task task)
        {
            lock (syncObj)
            {
                if (taskExecuting == 0)
                    waitAnyHandle.Reset();
                task.Completed += task_Completed;
                taskQueue.Enqueue(task);
                ProcessNextTask();
            }
        }
        void task_Completed(object sender, TaskCompletedEventArgs e)
        {
            lock (syncObj)
            {
                waitAnyHandle.Set();
                var task = sender as Task;
                task.Completed -= task_Completed;
                taskExecuting--;
                if (taskQueue.Count == 0 && taskExecuting == 0)
                    IsIdle = true;
                ProcessNextTask();
            }
        }
        void ProcessNextTask()
        {
            lock (syncObj)
            {
                if (taskQueue.Count == 1 && taskExecuting == 0)
                    IsIdle = false;
                if (taskExecuting < concurrency)
                {
                    var task = taskQueue.Dequeue();
                    if (task == null)
                        return;
                    try
                    {
                        threadPool.RunAsync(task);
                    }
                    catch (ObjectDisposedException e)
                    {
                        e.GetHashCode();
                    }
                    ++taskExecuting;
                }
            }
        }
        public void WaitAll()
        {
            WaitForIdle();
        }
        public bool WaitAll(int millisecondsTimeout)
        {
            return WaitForIdle(millisecondsTimeout);
        }
        public bool WaitAll(TimeSpan timeout)
        {
            return WaitForIdle(timeout);
        }
        public void WaitAny()
        {
            waitAnyHandle.WaitOne();
        }
        public bool WaitAny(int millisecondsTimeout)
        {
            return waitAnyHandle.WaitOne(millisecondsTimeout);
        }
        public bool WaitAny(TimeSpan timeout)
        {
            return waitAnyHandle.WaitOne(timeout);
        }
    }
}
