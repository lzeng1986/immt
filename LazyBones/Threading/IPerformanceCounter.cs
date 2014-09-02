using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Threading
{
    public interface IPerformanceCounter : IDisposable
    {
        void SampleThreads(long activeThreads, long workingThreads);
        void SampleTasks(long waitingTasks, long workingTasks);
        void SampleTaskWaitingTime(TimeSpan taskWaitingTime);
        void SampleTaskWorkingTime(TimeSpan taskWorkingTime);
    }
    public class PerformanceCounter : IPerformanceCounter
    {
        IPerformanceCounter[] array;
        TaskPerformanceCounters taskPerformanceCounters;
        ThreadPoolPerformanceCounters threadPoolPerformanceCounters;

        public TaskPerformanceCounters Task
        {
            get { return taskPerformanceCounters; }
        }
        public ThreadPoolPerformanceCounters ThreadPool
        {
            get { return threadPoolPerformanceCounters; }
        }

        public PerformanceCounter()
        {
            array = new IPerformanceCounter[] { taskPerformanceCounters, threadPoolPerformanceCounters };
        }

        void IPerformanceCounter.SampleThreads(long activeThreads, long workingThreads)
        {
            Array.ForEach(array, p => p.SampleThreads(activeThreads, workingThreads));
        }

        void IPerformanceCounter.SampleTasks(long waitingTasks, long workingTasks)
        {
            Array.ForEach(array, p => p.SampleTasks(waitingTasks, workingTasks));
        }

        void IPerformanceCounter.SampleTaskWaitingTime(TimeSpan taskWaitingTime)
        {
            Array.ForEach(array, p => p.SampleTaskWaitingTime(taskWaitingTime));
        }

        void IPerformanceCounter.SampleTaskWorkingTime(TimeSpan taskWorkingTime)
        {
            Array.ForEach(array, p => p.SampleTaskWorkingTime(taskWorkingTime));
        }

        void IDisposable.Dispose()
        {
            Array.ForEach(array, p => p.Dispose());
        }
    }
    public class TaskPerformanceCounters : IPerformanceCounter
    {
        void IPerformanceCounter.SampleThreads(long activeThreads, long workingThreads)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTasks(long waitingTasks, long workingTasks)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTaskWaitingTime(TimeSpan taskWaitingTime)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTaskWorkingTime(TimeSpan taskWorkingTime)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        TimeSpan avgWaitingTime;
        TimeSpan avgWorkingTime;
        int taskCount;
    }
    public class ThreadPoolPerformanceCounters : IPerformanceCounter
    {
        void IPerformanceCounter.SampleThreads(long activeThreads, long workingThreads)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTasks(long waitingTasks, long workingTasks)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTaskWaitingTime(TimeSpan taskWaitingTime)
        {
            throw new NotImplementedException();
        }

        void IPerformanceCounter.SampleTaskWorkingTime(TimeSpan taskWorkingTime)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
