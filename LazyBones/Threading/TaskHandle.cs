using System;
using System.Threading;
using System.Collections.Generic;

namespace LazyBones.Threading
{
    public abstract class TaskHandle : IDisposable
    {
        List<IPerformanceCounter> performanceCounters = new List<IPerformanceCounter>();

        protected void SampleThreads(long activeThreads, long workingThreads)
        {
            performanceCounters.ForEach(p => p.SampleThreads(activeThreads, workingThreads));
        }

        protected void SampleTasks(long waitingTasks, long workingTasks)
        {
            performanceCounters.ForEach(p => p.SampleTasks(waitingTasks, workingTasks));
        }

        protected void SampleTaskWaitingTime(TimeSpan taskWaitingTime)
        {
            performanceCounters.ForEach(p => p.SampleTaskWaitingTime(taskWaitingTime));
        }

        protected void SampleTaskWorkingTime(TimeSpan taskWorkingTime)
        {
            performanceCounters.ForEach(p => p.SampleTaskWorkingTime(taskWorkingTime));
        }

        public void AddPerformanceCounter(IPerformanceCounter performanceCounter)
        {
            if (IsIdle)
                performanceCounters.Add(performanceCounter);
            else
                throw new InvalidOperationException(Name + "正在执行");
        }
        public void RemovePerformanceCounter(IPerformanceCounter performanceCounter)
        {
            if (IsIdle)
                performanceCounters.Remove(performanceCounter);
            else
                throw new InvalidOperationException(Name + "正在执行");
        }
        public string Name { get; set; }

        public void WaitForIdle()
        {
            WaitForIdle(Timeout.Infinite);
        }

        public bool WaitForIdle(TimeSpan timeout)
        {
            return WaitForIdle((int)timeout.TotalMilliseconds);
        }

        public virtual bool WaitForIdle(int millisecondsTimeout)
        {
            return idleWaitHandle.WaitOne(millisecondsTimeout);
        }

        ManualResetEvent idleWaitHandle = new ManualResetEvent(true);
        bool isIdle = true;
        public bool IsIdle
        {
            get { return isIdle; }
            protected set
            {
                if (isIdle == value)
                    return;
                isIdle = value;
                if (isIdle)
                {
                    idleWaitHandle.Set();
                    OnIdle();
                }
                else
                {
                    idleWaitHandle.Reset();
                }
            }
        }

        public event EventHandler Idle;

        void OnIdle()
        {
            var handler = Idle;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public abstract int Concurrency { get; set; }

        public void Close()
        {
            CheckDispose();
            Dispose();
            CloseInternal(false, Timeout.Infinite);
        }
        public void Close(int timeout)
        {
            CheckDispose();
            Dispose();
            CloseInternal(true, timeout);
        }
        protected abstract void CloseInternal(bool forceAbort, int timeout);

        bool isDispose = false;

        public void Dispose()
        {
            if (!isDispose)
            {
                isDispose = true;
                performanceCounters.ForEach(p => p.Dispose());
                Close();
            }
        }
        void CheckDispose()
        {
            if (isDispose)
                throw new ObjectDisposedException(GetType().Name);
        }
        public Task RunAsync(Action action, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task(action) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public Task RunAsync<T>(Action<T> action, T arg, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task(action, arg) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public Task RunAsync<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task(action, arg1, arg2) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public Task<TResult> RunAsync<TResult>(Func<TResult> action, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task<TResult>(action) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public Task<TResult> RunAsync<T, TResult>(Func<T, TResult> action, T arg, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task<TResult>(action, arg) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public Task<TResult> RunAsync<T1, T2, TResult>(Func<T1, T2, TResult> action, T1 arg1, T2 arg2, TaskPriority priority = TaskPriority.Normal)
        {
            var task = new Task<TResult>(action, arg1, arg2) { Priority = priority };
            RunAsync(task);
            return task;
        }
        public abstract void RunAsync(Task task);

        public void ForEach(IEnumerable<Action> actions)
        {
            foreach (var a in actions)
                RunAsync(a);
        }
        public void ForEach<T>(IEnumerable<Action<T>> actions, T arg)
        {
            foreach (var a in actions)
                RunAsync(a, arg);
        }
        public void ForEach<T>(IEnumerable<T> args,Action<T> action)
        {
            foreach (var a in args)
                RunAsync(action, a);
        }
    }
}
