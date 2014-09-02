using System;
using System.Diagnostics;
using System.Threading;
using LazyBones.Extensions;

namespace LazyBones.Threading
{
    /// <summary>
    /// 表示线程池内作业 [10/23/2013 zliang]
    /// </summary>
    public class ThreadJob
    {
        Stopwatch waitWatch = new Stopwatch();
        Stopwatch inQueueWatch = new Stopwatch();
        Stopwatch workingWatch = new Stopwatch();
        ThreadJobStatus status = ThreadJobStatus.Created;
        object lockObj = new object();
        public TimeSpan WaitingTime
        {
            get { return waitWatch.Elapsed; }
        }
        public TimeSpan InQueueTime
        {
            get { return inQueueWatch.Elapsed; }
        }
        public TimeSpan WorkingTime
        {
            get { return workingWatch.Elapsed; }
        }
        public ThreadJobStatus Status
        {
            get
            {
                lock (lockObj)
                {
                    return status;
                }
            }
            private set
            {
                lock (lockObj)
                {
                    if (!CheckStatusChange(value))
                        return;
                    status = value;
                    FireEvents();
                    if (status == ThreadJobStatus.Canceled || status == ThreadJobStatus.Error || status == ThreadJobStatus.Finished)
                        Monitor.PulseAll(lockObj);
                }
            }
        }
        bool CheckStatusChange(ThreadJobStatus changeTo)
        {
            if (status == changeTo) // 不能转换到相同状态 [10/30/2013 zliang]
                return false;
            switch (status)
            {
                case ThreadJobStatus.Created: // 这是初始状态，可以转换到任何状态 [10/30/2013 zliang]
                    return true;
                case ThreadJobStatus.Canceled:
                case ThreadJobStatus.Finished:
                case ThreadJobStatus.Error:   //这三个是最终状态，不允许做任何改变
                    return false;
                case ThreadJobStatus.Executing:
                    return (changeTo == ThreadJobStatus.Canceled) || (changeTo == ThreadJobStatus.Finished) || (changeTo == ThreadJobStatus.Error);
                case ThreadJobStatus.InQueue:
                    return changeTo != ThreadJobStatus.Created;
                default:
                    return false;
            }
        }
        public event FinishedHandler Finished;
        public event CanceledHandler Canceled;
        public event ExecutedHandler Executed;
        public event ErrorOccuredHandler ErrorOccured;
        void FireEvents()
        {
            switch (status)
            {
                case ThreadJobStatus.Canceled:
                    OnCancel();
                    break;
                case ThreadJobStatus.Finished:
                    OnFinish();
                    break;
                case ThreadJobStatus.Executing:
                    OnExecute();
                    break;
                case ThreadJobStatus.Error:
                    OnErrorOccure();
                    break;
            }
        }
        void OnCancel()
        {
            var handler = Canceled;
            if (handler != null)
                handler(this);
        }
        void OnFinish()
        {
            var handler = Finished;
            if (handler != null)
                handler(this);
        }
        void OnExecute()
        {
            var handler = Executed;
            if (handler != null)
                handler(this);
        }
        void OnErrorOccure()
        {
            var handler = ErrorOccured;
            if (handler != null)
                handler(this);
        }
        public string Name { get; set; }
        public Exception Exception { get; private set; }
        ThreadJob nextJob = null;
        readonly Delegate job;
        readonly object[] arg;
        internal Thread workingThread = null;
        ManualResetEvent completeWaitHandle = new ManualResetEvent(false);

        public ThreadJob(Action job)
        {
            this.job = job;
            this.arg = new object[0];
            waitWatch.Start();
        }
        public ThreadJob(Action<object> job, object arg)
        {
            this.job = job;
            this.arg = new[] { arg };
            waitWatch.Start();
        }
        public ThreadJob(Delegate job, params object[] args)
        {
            this.job = job;
            this.arg = args;
            waitWatch.Start();
        }
        /// <summary>
        /// 如果当前作业已完成，则直接执行next，如果当前作业未完成，则等待当前作业完成之后再执行next
        /// </summary>
        /// <param name="next">当前作业的下一个作业</param>
        public void Next(ThreadJob next)
        {
            if (next == null)
                throw new ArgumentNullException("next");
            nextJob = next;
            StartNextJob();
        }
        public bool IsCompleted
        {
            get
            {
                lock (lockObj)
                {
                    return status == ThreadJobStatus.Finished || status == ThreadJobStatus.Error;
                }
            }
        }
        public bool IsCanceled
        {
            get { return Status == ThreadJobStatus.Canceled; }
        }
        public bool HasError
        {
            get { return Status == ThreadJobStatus.Error; }
        }
        internal bool IsExecutable()    //检查是否可以开始执行
        {
            if (IsCanceled || IsCompleted)
                return false;

            if (inQueueWatch.IsRunning)
                inQueueWatch.Stop();

            workingWatch.Start();

            Status = ThreadJobStatus.Executing;
            return true;
        }
        public void Cancel()    //取消作业
        {
            lock (lockObj)
            {
                if (status == ThreadJobStatus.Executing)
                {
                    workingThread.Abort();
                }

                Status = ThreadJobStatus.Canceled;

                if (waitWatch.IsRunning)
                    waitWatch.Stop();
                if (inQueueWatch.IsRunning)
                    inQueueWatch.Stop();
                if (workingWatch.IsRunning)
                    workingWatch.Stop();
            }
        }
        internal void IsQueued()
        {
            lock (lockObj)
            {
                waitWatch.Stop();
                inQueueWatch.Start();
                Status = ThreadJobStatus.InQueue;
            }
        }
        internal void Execute()
        {
            try
            {
                job.DynamicInvoke(arg);
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (System.Exception ex)
            {
                Exception = ex;
            }
            workingWatch.Stop();
            Status = Exception == null ? ThreadJobStatus.Finished : ThreadJobStatus.Error;
            StartNextJob();    //作业完成之后，开始Next作业
        }
        void StartNextJob()
        {
            if (IsCompleted && nextJob != null && !nextJob.IsCanceled)
            {
                LBThreadPool.Default.RunAsync(nextJob);
            }
        }
        public void WaitToComplete()
        {
            lock (lockObj)
            {
                if (IsCompleted)
                    return;
                Monitor.Wait(lockObj);
            }
        }
        public bool WaitToComplete(int millisecondsTimeout)
        {
            return WaitToComplete(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }
        public bool WaitToComplete(TimeSpan timeout)
        {
            lock (lockObj)
            {
                if (IsCompleted)
                    return true;
                return Monitor.Wait(lockObj, timeout);
            }
        }
        public void Start()
        {
            Start(LBThreadPool.Default);
        }
        public void Start(LBThreadPool threadPool)
        {
            threadPool.RunAsync(this);
        }
        public static ThreadJob Create(Action action)
        {
            return new ThreadJob(action);
        }
        public static ThreadJob Create<T>(Action<T> action, T arg)
        {
            return new ThreadJob(action, arg);
        }
    }
}
