using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LazyBones.Utils;

namespace LazyBones.Threading
{
    //带优先级的队列，不保证线程安全
    class PriorityQueue : IEnumerable<Task>
    {
        //使用链表可以减少频繁增加删除带来的性能压力
        LinkedList<Task>[] queues = new LinkedList<Task>[TaskPriority.Highest - TaskPriority.Lowest + 1];
        int count = 0;
        public PriorityQueue()
        {
            for (var i = 0; i < queues.Length; i++)
                queues[i] = new LinkedList<Task>();
        }

        public void Enqueue(Task task)
        {
            ParamGuard.NotNull(task, "task");
            var ind = TaskPriority.Highest - task.Priority;
            Debug.Assert(0 <= ind && ind <= (int)TaskPriority.Highest);
            queues[ind].AddLast(task);
            count++;
        }

        public Task Dequeue()
        {
            for (var i = 0; i < queues.Length; i++)
            {
                if (queues[i].Count > 0)
                {
                    var task = queues[i].First.Value;
                    queues[i].RemoveFirst();
                    count--;
                    return task;
                }
            }
            return null;
        }

        public int Count { get { return count; } }

        public void Clear()
        {
            Array.ForEach(queues, q => q.Clear());
            count = 0;
        }

        public IEnumerator<Task> GetEnumerator()
        {
            return queues.SelectMany(q => q).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
