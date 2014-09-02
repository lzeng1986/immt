using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Linq;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Gantt
{
    public class GanttTaskCollection : IEnumerable<GanttTask>
    {
        SmartList<GanttTask> tasks = new SmartList<GanttTask>();//该队列始终保持有序
        public void Add(GanttTask task)//插入时保证有序性
        {
            ParamGuard.NotNull(task, "task");
            if (task.Collection != null)
                task.Collection.Remove(task);
            tasks.Add(task);
            task.Collection = this;
        }
        public void Remove(GanttTask task)
        {
            ParamGuard.NotNull(task, "task");
            tasks.Remove(task);
            task.Collection = null;
        }
        public IEnumerator<GanttTask> GetEnumerator()
        {
            return tasks.Concat(tasks.SelectMany(t=>t.AllChildren)).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void RecalcComplete()
        {
            tasks.Where(t => t.HasChildren).ForEach(t => t.RecalcCompleted());
        }
        public void RecalcSchedule()
        {
            tasks.Where(t => t.HasChildren).ForEach(t => t.RecalcChildrenSchedule());
        }
    }
}
