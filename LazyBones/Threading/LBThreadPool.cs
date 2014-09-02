using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LazyBones.Log;
using LazyBones.Utils;
using System.Runtime.CompilerServices;
using System.Security;

namespace LazyBones.Threading
{
    /// <summary>
    /// 提供一个可实例化、可调度的线程池 [10/9/2013 zliang]
    /// </summary>
    public class LBThreadPool : TaskHandle
    {
        public const int DefaultMinThreads = 0;
        public static readonly int DefaultMaxThreads = Environment.ProcessorCount * 20;// 初始值每个处理器最多可拥有20个线程 [10/9/2013 zliang]
        public const ThreadPriority DefaultThreadPriority = ThreadPriority.Normal;
        public const int DefaultThreadIdleTimeOut = 30 * 60 * 1000;//线程最大空闲时间30分钟

        int maxThreads = DefaultMaxThreads;
        int minThreads = DefaultMinThreads;
        int threadIdleTimeOut;
        int workingThreadsCount = 0;//正在执行Task的线程数量
        int workingTaskCount = 0;//正在执行的Task数量
        int taskCount = 0;//=workingTaskCount+workingQueue.Count 线程池中Task总数量
        bool active = true;
        WaitingQueue taskWaitingQueue = new WaitingQueue();
        SyncDictionary<Thread, ThreadEntry> workThreads = new SyncDictionary<Thread, ThreadEntry>();
        LinkedList<ThreadEntry> idleEntryList = new LinkedList<ThreadEntry>();
        ThreadPoolInfo poolInfo = new ThreadPoolInfo();
        DateTime lastIdleTime = DateTime.Now;
        DateTime? lastWorkingTime;

        ManualResetEvent closeWaitHandle = new ManualResetEvent(false);
        IPerformanceCounter performanceCounter = new PerformanceCounter();
        Dictionary<string, TaskQueue> parallels = new Dictionary<string, TaskQueue>();

        public ThreadPriority Priority { get; set; }
        public int ThreadIdleTimeout
        {
            get { return threadIdleTimeOut; }
        }
        public PerformanceCounter PerformanceCounter
        {
            get { return (PerformanceCounter)performanceCounter; }
        }

        static long ThreadPoolSequenceNo = 0;
        static long ThreadSequenceNo = 0;

        public LBThreadPool()
            : this("#" + Interlocked.Increment(ref ThreadPoolSequenceNo))
        {
        }
        public LBThreadPool(string name)
            : this(name, DefaultThreadPriority)
        {

        }
        public LBThreadPool(string name, ThreadPriority priority)
            : this(name, priority, DefaultThreadIdleTimeOut)
        {
        }
        public LBThreadPool(string name, ThreadPriority priority, int threadIdleTimeOut)
        {
            Priority = priority;
            Name = name;
            this.threadIdleTimeOut = threadIdleTimeOut;
            StartNewThread(minThreads);//初始化为最小线程数量
        }
        void StartNewThread(int count)
        {
            if (count <= 0)
                return;
            lock (workThreads.SyncRoot)
            {
                for (var i = 0; i < count; i++)
                {
                    if (workThreads.Count >= maxThreads)
                        break;
                    var thread = new Thread(ProcessQueueJob);
                    thread.IsBackground = true;
                    thread.Name = string.Format("{0}~{1}", Name, Interlocked.Increment(ref ThreadSequenceNo));
                    workThreads[thread] = new ThreadEntry(this);
                    thread.Start();
                }
            }
        }
        protected override void CloseInternal(bool forceAbort, int timeout)
        {
            Thread[] threads;
            lock (workThreads.SyncRoot)
            {
                taskWaitingQueue.Dispose();

                active = false;
                closeWaitHandle.Set();

                threads = workThreads.Keys.ToArray();
                workThreads.Clear();
            }
            if (CloseThreads(timeout, threads))
                return;
            if (forceAbort)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }
            }
        }
        bool CloseThreads(int timeout, Thread[] threads)
        {
            if (timeout == Timeout.Infinite)
            {
                foreach (Thread thread in threads)
                    thread.Join();
                return true;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                foreach (Thread thread in threads)
                {
                    var millisecondsLeft = timeout - stopwatch.ElapsedMilliseconds;
                    if (millisecondsLeft < 0)
                    {
                        return false;
                    }

                    if (!thread.Join((int)millisecondsLeft))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 获取线程池最近一次运行时长
        /// </summary>
        public TimeSpan ExeTime
        {
            get { return lastWorkingTime.HasValue ? (DateTime.Now - lastWorkingTime.Value) : TimeSpan.Zero; }
        }
        /// <summary>
        /// 获取线程池最近一次空闲时长
        /// </summary>
        public TimeSpan IdleTime
        {
            get { return DateTime.Now - lastIdleTime; }
        }

        /// <summary>
        /// 获取或设置线程池最大可并行执行<see cref="Task"/>数量
        /// </summary>
        public override int Concurrency
        {
            get { return maxThreads; }
            set
            {
                maxThreads = Math.Max(value, minThreads);
                ModifyThreadNum();
            }
        }
        /// <summary>
        /// 获取或设置线程池最小线程数量
        /// </summary>
        public int MinThreads
        {
            get { return minThreads; }
            set
            {
                minThreads = Math.Max(value, 0);
                maxThreads = Math.Max(maxThreads, minThreads);
                ModifyThreadNum();
            }
        }

        void ModifyThreadNum() // 根据当前等在作业数量调整池内线程数量 [10/14/2013 zliang]
        {
            var count = Math.Min(taskWaitingQueue.Count, maxThreads);
            count = Math.Max(minThreads, count);
            var diff = count - workThreads.Count;
            if (diff > 0)
            {
                StartNewThread(diff);
            }
        }

        public override void RunAsync(Task task)
        {
            ParamGuard.NotNull(task, "task");
            taskWaitingQueue.Enqueue(task);
            IncrementTask();
            if (taskCount > workThreads.Count)    // 如果作业数量大于等于当前线程数量，则增加一个线程用于处理当前作业 [10/11/2013 zliang]
            {
                StartNewThread(1);
            }
        }

        bool TryGetTask(out Task task)//尝试获取任务，如果等待超时或者线程池关闭，则获取失败
        {
            task = taskWaitingQueue.Dequeue(threadIdleTimeOut, closeWaitHandle);
            return task != null;
        }

        void ProcessQueueJob()  //线程执行函数
        {
            var currentEntry = workThreads[Thread.CurrentThread];
            ThreadEntry.Current = currentEntry;
            try
            {
                while (active)
                {
                    if (workThreads.Count > maxThreads)
                    {
                        lock (workThreads.SyncRoot)// 如果线程数已大于最大线程数，则直接退出该线程 [10/11/2013 zliang]
                        {
                            if (workThreads.Count > maxThreads)
                            {
                                ThreadCompleted();
                                break;
                            }
                        }
                    }

                    Task task;
                    if (TryGetTask(out task))
                    {
                        try
                        {
                            if (task.CheckReadyForExecute())  // 这一句之后的取消作业操作是调用Abort方法结束线程 [10/14/2013 zliang]
                            {
                                Interlocked.Increment(ref workingThreadsCount);
                                SampleThreads(workThreads.Count, workingThreadsCount);
                                currentEntry.Refresh();
                                currentEntry.IsWorking = true;
                                ExecuteTask(task);
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            Thread.ResetAbort();//在调用Abort方法结束线程之后，恢复该线程用于后续作业，而非真正结束该线程
                        }
                        finally
                        {
                            Interlocked.Decrement(ref workingThreadsCount);
                            currentEntry.IsWorking = false;
                            DecrementTask();
                        }
                    }
                    else
                    {
                        if (workThreads.Count > minThreads)
                        {
                            lock (workThreads.SyncRoot)// 如果线程数大于最小线程数，则结束该线程 [10/11/2013 zliang]
                            {
                                if (workThreads.Count > minThreads)
                                {
                                    ThreadCompleted();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TinyLog.Fatal("线程池执行函数异常退出：" + ex);
                Debug.Assert(false);// 这里是不应该到达的地方 [10/14/2013 zliang]
                throw;
            }
            finally // 线程结束，由于.net保证在finally块中是线程安全的，因此这里不必加锁 [10/14/2013 zliang]
            {
                ThreadCompleted();
                currentEntry.Dispose();
                ThreadEntry.Current = null;
            }
        }
        void ThreadCompleted()
        {
            if (workThreads.Remove(Thread.CurrentThread))
            {
                SampleThreads(workThreads.Count, workingThreadsCount);
            }
        }
        void ExecuteTask(Task task)
        {
            SampleTaskWaitingTime(task.WaitingTime);
            Interlocked.Increment(ref workingTaskCount);
            SampleTasks(taskWaitingQueue.Count, workingTaskCount);
            task.Execute();
            SampleTaskWorkingTime(task.WorkingTime);
        }

        void IncrementTask()
        {
            if (Interlocked.Increment(ref taskCount) == 1)
            {
                IsIdle = false;
                lastWorkingTime = DateTime.Now;
            }
            SampleTasks(taskWaitingQueue.Count, workingTaskCount);
        }

        void DecrementTask()
        {
            var count = Interlocked.Decrement(ref taskCount);
            Interlocked.Decrement(ref workingTaskCount);
            if (active)
            {
                SampleTasks(taskWaitingQueue.Count, workingTaskCount);
            }

            if (count == 0)
            {
                IsIdle = true;
                lastIdleTime = DateTime.Now;
            }
        }

        public TaskQueue GetParallel(string name)
        {
            TaskQueue parallel;
            if (parallels.TryGetValue(name, out parallel))
            {
                return parallel;
            }
            parallel = new TaskQueue(this) { Name = name };
            parallels.Add(name, parallel);
            return parallel;
        }
        internal void RemoveParallel(TaskQueue queue)
        {
            parallels.Remove(queue.Name);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ValidateTaskHandleWaitForIdle(TaskHandle handle)
        {
            if (null == ThreadEntry.Current)
                return;

            var task = ThreadEntry.Current.Task;
            if (null != task && task.Handle == handle)
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its SmartThreadPool, it causes a deadlock");
            }
        }

        //实体，用于线程池内部处理
        class ThreadEntry : IDisposable
        {
            [ThreadStatic]
            public static ThreadEntry Current;

            public bool IsWorking = false;

            public DateTime LastAliveTime { get; private set; }

            public DateTime CreateTime { get; private set; }

            public LBThreadPool AssociatedLBThreadPool { get; private set; }

            public void Refresh()
            {
                LastAliveTime = DateTime.Now;
            }
            public Task Task { get; set; }
            public ThreadEntry(LBThreadPool threadPool)
            {
                CreateTime = DateTime.Now;
                LastAliveTime = DateTime.MinValue;
                AssociatedLBThreadPool = threadPool;
            }
            bool isDisposed = false;
            public void Dispose()
            {
                lock (this)
                {
                    if (!isDisposed)
                    {
                        isDisposed = true;
                    }
                }
            }
        }
    }
}
