using System;
using System.Collections.Generic;
using System.Threading;
using LazyBones.Utils;

namespace LazyBones.Threading
{
    /// <summary>
    /// 用于后台处理作业队列 [by zliang]
    /// </summary>
    public class BackgroundQueueWorker : IDisposable
    {
        LinkedList<Task> jobQueue = new LinkedList<Task>();
        int capacity;
        AutoResetEvent syncObj = new AutoResetEvent(false);
        Thread workThread;
        volatile JobAppendMode appendMode = JobAppendMode.Drop;
        volatile bool isDisposed = false;

        public BackgroundQueueWorker(int capacity)
        {
            this.capacity = capacity;
            workThread = new Thread(ProcessQueueJob) { IsBackground = true };
            workThread.Start();
        }
        /// <summary>
        /// 获取或设置当内部队列满时，新作业的添加模式
        /// </summary>
        public JobAppendMode AppendMode
        {
            get { return appendMode; }
            set { appendMode = value; }
        }
        /// <summary>
        /// 获取内部等待作业队列数量
        /// </summary>
        public int WaitJobNum
        {
            get
            {
                lock (jobQueue)
                {
                    return jobQueue.Count;
                }
            }
        }
        /// <summary>
        /// 将委托方法加入线程池执行
        /// </summary>
        /// <param name="callBack">执行的委托</param>
        /// <returns>此委托对应的线程池作业</returns>
        public Task RunAsync(Action callBack)
        {
            var job = new Task(callBack);
            RunAsync(job);
            return job;
        }
        /// <summary>
        /// 将带参数的委托方法加入线程池执行
        /// </summary>
        /// <typeparam name="T">委托参数类型</typeparam>
        /// <param name="callBack">执行的委托</param>
        /// <param name="state">委托参数</param>
        /// <returns>此委托对应的线程池作业</returns>
        public Task RunAsync<T>(Action<T> callBack, T state)
        {
            var job = new Task(callBack, state);
            RunAsync(job);
            return job;
        }
        /// <summary>
        /// 将一个线程池作业加入线程池执行
        /// </summary>
        /// <param name="job">需要执行的线程池作业</param>
        public void RunAsync(Task job)
        {
            ParamGuard.NotNull(job, "job");
            if (isDisposed)
                throw new ObjectDisposedException("BackgroundQueueWorker");
            lock (jobQueue)
            {
                if (jobQueue.Count == capacity)
                {
                    switch (appendMode)
                    {
                        case JobAppendMode.Drop:
                            return;
                        case JobAppendMode.RemoveHead:
                            jobQueue.RemoveFirst();
                            break;
                    }
                }
                jobQueue.AddLast(job);
                syncObj.Set();
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;
            syncObj.Set();
            lock (jobQueue)
            {
                jobQueue.Clear();
            }
            try
            {
                workThread.Join();
            }
            catch { }
            workThread = null;
        }
        void ProcessQueueJob()
        {
            while (syncObj.WaitOne())
            {
                if (isDisposed)
                    break;
                Task job;
                lock (jobQueue)
                {
                    job = jobQueue.First.Value;
                    jobQueue.RemoveFirst();
                }
                try
                {
                    job.Execute();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch
                {
                }
            }
        }
    }

    /// <summary>
    /// 表示<see cref="BackgroundQueueWorker"/>内部队列满时，新作业添加模式
    /// </summary>
    public enum JobAppendMode
    {
        /// <summary>
        /// 直接丢弃
        /// </summary>
        Drop,

        /// <summary>
        /// 删除最早的作业，然后添加至尾部
        /// </summary>
        RemoveHead
    }
}
