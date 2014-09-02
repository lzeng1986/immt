using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Threading
{
    public static class TaskFactory
    {
        public static Task New(Action action)
        {
            return new Task(action);
        }
        public static Task New<T>(Action<T> action, T arg)
        {
            return new Task(action, arg);
        }
        public static Task New<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            return new Task(action, arg1, arg2);
        }
    }
}
