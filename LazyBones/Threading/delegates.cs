using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Threading
{
    public class TaskEventArgs : EventArgs
    {
        public Task Task { get;private set; }
        public TaskEventArgs(Task task)
        {
            Task = task;
        }
    }
    public class TaskCompletedEventArgs : EventArgs
    {
        public CompleteReason Reason { get; private set; }
        public TaskCompletedEventArgs(CompleteReason reason)
        {
            Reason = reason;
        }
    }
}
