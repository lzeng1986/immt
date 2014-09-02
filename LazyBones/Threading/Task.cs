using System;
using System.Diagnostics;
using System.Threading;

namespace LazyBones.Threading
{
    /// <summary>
    /// 表示线程池内作业 [10/23/2013 zliang]
    /// </summary>
    public class Task : IDisposable
    {
        Stopwatch waitingWatch = new Stopwatch();
        Stopwatch workingWatch = new Stopwatch();
        volatile TaskStatus status;
        object lockObj = new object();
        Thread workingThread = null;
        ManualResetEvent completeWaitHandle = new ManualResetEvent(false);

        protected Delegate executor;
        protected object[] args;

        internal TaskHandle Handle;

        internal protected Task(Delegate executor, params object[] args)
        {
            this.executor = executor;
            this.args = args;
            Priority = TaskPriority.Normal;
        }

        public bool IsCompleted
        {
            get { return status == TaskStatus.Completed; }
        }
        public bool IsCanceled
        {
            get { return status == TaskStatus.Canceled; }
        }
        void CheckModify()
        {
            if (status == TaskStatus.Working)
                throw new NotSupportedException("任务正在执行，不得修改状态");
        }

        public TimeSpan WaitingTime
        {
            get { return waitingWatch.Elapsed; }
        }
        public TimeSpan WorkingTime
        {
            get { return workingWatch.Elapsed; }
        }
        public TaskStatus Status
        {
            get { return status; }
            private set
            {
                if (!Helper.CheckJobStatusChange(status, value))
                    return;
                status = value;
            }
        }
        public event EventHandler<TaskCompletedEventArgs> Completed;
        public string Name { get; set; }
        public Exception Exception { get; protected set; }
        public void Cancel()    //取消作业
        {
            lock (lockObj)
            {
                if (status == TaskStatus.Working)
                {
                    workingThread.Abort();
                    workingWatch.Stop();
                }
                workingThread = null;
                Status = TaskStatus.Canceled;
                OnCompleted(CompleteReason.Normal);
                waitingWatch.Stop();
            }
        }
        internal bool CheckReadyForExecute()    //检查是否可以开始执行
        {
            lock (lockObj)
            {
                if (IsCanceled)
                    return false;
                Debug.Assert(status == TaskStatus.Waiting);
                waitingWatch.Stop();
                workingThread = Thread.CurrentThread;
                Status = TaskStatus.Working;
                return true;
            }
        }
        internal void Queued()//指示任务放入了队列
        {
            lock (lockObj)
            {
                waitingWatch.Start();
                Status = TaskStatus.Waiting;
            }
        }

        internal void Execute()//任务执行
        {
            try
            {
                workingWatch.Start();
                OnExecute();
                OnCompleted(CompleteReason.Normal);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                Exception = ex;
                OnCompleted(CompleteReason.Error);
            }
            finally
            {
                workingThread = null;
                workingWatch.Stop();
                completeWaitHandle.Set();
                status = TaskStatus.Completed;
                
            }
        }
        protected void OnCompleted(CompleteReason reason)
        {
            var handle = Completed;
            try
            {
                if (handle != null)
                    handle(this, new TaskCompletedEventArgs(reason));
            }
            catch { }
        }
        protected virtual void OnExecute()
        {
            executor.DynamicInvoke(args);
        }

        public void Wait()
        {
            Wait(Timeout.Infinite);
        }
        public bool Wait(TimeSpan timeout)
        {
            return Wait((int)timeout.TotalMilliseconds);
        }
        public bool Wait(int millisecondsTimeout)
        {
            return completeWaitHandle.WaitOne(millisecondsTimeout);
        }

        public TaskPriority Priority { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    public class Task<TResult> : Task
    {
        internal Task(Delegate executor, params object[] args)
            : base(executor, args)
        {
        }
        public TResult Result { get; private set; }
        protected override void OnExecute()
        {
            Result = (TResult)executor.DynamicInvoke(args);
        }
    }
}
