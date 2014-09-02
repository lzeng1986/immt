using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LazyBones.Threading
{
    public interface IPriority
    {
        TaskPriority Priority { get; }
    }
    public interface ITaskExecutor : IDisposable
    {
        ThreadPriority Priority { get; set; }

        string Name { get; set; }

        int Concurrency { get; set; }

        int WaitingTasks { get; }

        int WorkingTasks { get; }

        int TaskCount { get; }

        void Start();

        void Cancel();

        void CancelWithExecution();

        void WaitForIdle();

        bool WaitForIdle(TimeSpan timeout);

        bool WaitForIdle(int millisecondsTimeout);

        bool IsIdle { get; }

        event EventHandler OnIdle;
    }
}
