using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Linq;
using System.Collections;

namespace LazyBones.UI.Controls.Gantt
{
    public class GanttTask : IComparable<GanttTask>
    {
        public GanttTaskCollection Collection { get; internal set; }

        public string Name { get; set; }

        public bool IsCollapsed { get; set; }

        double complete;
        public double Complete
        {
            get { return complete; }
            set
            {
                //拥有part或者child的task只能直接更改Complete
                if (Collection == null || complete == value || HasChildren || HasParts)
                    return;

                complete = value;
                complete = Math.Min(Math.Max(0, complete), 1);//将值调整到0~1之间

                var task = wholeTask ?? parentTask;
                if (task != null)
                    task.RecalcCompleted();
            }
        }

        DateTime start;
        public DateTime Start
        {
            get { return start; }
            set
            {
                //如果有child，则不能直接更改start
                if (Collection == null || start == value || HasChildren)
                    return;

                //根据前继节点调整start值
                value = value.Concat((wholeTask ?? this).predecessorTask.Select(t => t.end)).Max();
                var off = value - start;
                start += off;
                end += off;

                if (IsPart)
                {
                    if (off < TimeSpan.Zero)
                        wholeTask.PackPartsBackwards();
                    else
                        wholeTask.PackPartsForward();

                    RecalcWholeTaskTime();
                }
                else
                {
                    parts.ForEach(t =>
                    {
                        t.start += off;
                        t.end += off;
                    });
                }
                (wholeTask ?? this).RecalcSuccessorStart();
            }
        }

        DateTime end;
        public DateTime End
        {
            get { return end; }
            set
            {
                if (Collection == null || end == value || HasChildren)
                    return;
                if (value < start)//value不得小于Start
                    value = start;
                if (IsPart)//向前移动part，不做任何处理，向后移动则调用PackPartsForward进行调整
                {
                    var increased = value > end;
                    end = value;
                    duration = end - start;

                    if (increased)
                        wholeTask.PackPartsForward();

                    RecalcWholeTaskTime();
                }
                else if (HasParts)
                {
                    var lastPart = parts.Last();
                    if (value < lastPart.Start)//value不得小于最后一个part的Start
                        value = lastPart.Start;

                    end = value;
                    duration = end - start;

                    lastPart.end = value;
                    lastPart.duration = lastPart.End - lastPart.Start;
                }
                else
                {
                    end = value;
                    duration = end - start;
                }
                (wholeTask ?? this).RecalcSuccessorStart();
            }
        }

        TimeSpan duration = TimeSpan.Zero;
        public TimeSpan Duration
        {
            get { return duration; }
            set { End = start + value; }
        }

        public TimeSpan Slack { get; private set; }

        HashSet<object> resources = new HashSet<object>();
        public IEnumerable<object> Resources
        {
            get { return resources; }
        }
        public IEnumerable<object> AllResources
        {
            get
            {
                if (HasChildren)
                    return resources.Concat(AllChildren.SelectMany(c => c.resources)).Distinct();
                if (HasParts)
                    return resources.Concat(parts.SelectMany(c => c.resources)).Distinct();
                return resources;
            }
        }
        public override string ToString()
        {
            return string.Format("[Name = {0}, Start = {1}, End = {2}, Duration = {3}, Complete = {4}]", Name, Start, End, Duration, Complete);
        }

        SmartList<GanttTask> children = new SmartList<GanttTask>();
        List<GanttTask> parts = new List<GanttTask>();
        SmartList<GanttTask> successorTask = new SmartList<GanttTask>();
        SmartList<GanttTask> predecessorTask = new SmartList<GanttTask>();
        GanttTask parentTask;
        GanttTask wholeTask;
        public bool HasChildren
        {
            get { return children.Count > 0; }
        }
        public bool HasParts
        {
            get { return parts.Count > 0; }
        }
        public bool IsChild
        {
            get { return parentTask != null; }
        }
        public bool IsPart
        {
            get { return parentTask != null; }
        }
        public bool HasRelation
        {
            get { return successorTask.Count > 0 && predecessorTask.Count > 0; }
        }
        public IEnumerable<GanttTask> Children
        {
            get { return children; }
        }
        public IEnumerable<GanttTask> AllChildren
        {
            get { return children.Concat(children.SelectMany(c => c.AllChildren)); }
        }

        public void AddChild(GanttTask child)
        {
            if (child == null || child == this || HasParts || IsPart || HasRelation)
                return;

            child = child.wholeTask ?? child;//如果child是另一个task的一部分，则将针对task进行操作

            if (child.AllChildren.Contains(this))//防止循环添加
                return;

            if (child.parentTask != null)
                child.parentTask.RemoveChild(child);

            if (children.Add(child))
            {
                child.parentTask = this;
                RecalcChildrenSchedule(); 
            }
        }
        public void RemoveChild(GanttTask child)
        {
            if (child == null || child == this)
                return;
            child = wholeTask ?? child;//如果child是另一个task的一部分，则将针对task进行操作
            children.Remove(child);
        }
        public void RelateFrom(GanttTask predecessor)
        {
            if (predecessor == null || Collection == null || predecessor.Collection != Collection)
                return;
            var from = predecessor.wholeTask ?? predecessor;
            var to = wholeTask ?? this;
            Relate(from, to);
        }
        public void RelateTo(GanttTask successor)
        {
            if (successor == null || Collection == null || successor.Collection != Collection)
                return;
            var from = wholeTask ?? this;
            var to = successor.wholeTask ?? successor;
            Relate(from, to);
        }
        public void UnrelateFrom(GanttTask predecessor)
        {
            if (predecessor == null || Collection == null)
                return;
            var from = predecessor.wholeTask ?? predecessor;
            var to = wholeTask ?? this;
            UnRelate(from, to);
        }
        public void UnrelateTo(GanttTask successor)
        {
            if (successor == null || Collection == null)
                return;
            var from = wholeTask ?? this;
            var to = successor.wholeTask ?? successor;
            UnRelate(from, to);
        }
        static void Relate(GanttTask from, GanttTask to)
        {
            if (from.successorTask.Add(to))
            {
                from.RecalculateSlack();
            }
        }
        static void UnRelate(GanttTask from, GanttTask to)
        {
            if (from.successorTask.Remove(to))
            {
                from.RecalculateSlack();
            }
        }
        public void Split(GanttTask task, GanttTask part1, GanttTask part2, TimeSpan duration)
        {
            //if (task == null || part1 == null || part2 == null || part1 == part2)
            //    return;
            //if (registerTask.Contains(task) // task must be registered
            //    && !splitTasks.ContainsKey(task) // task must not already be a split task
            //    && !splitTaskParents.ContainsKey(task) // task must not be a task part
            //    && taskChildren[task].Count == 0 // task cannot be a group
            //    && !registerTask.Contains(part1) // part1 and part2 must have never existed
            //    && !registerTask.Contains(part2)
            //    )
            //{
            //    registerTask.Add(part1);  // register part1
            //    resources[part1] = new HashSet<Object>(); // create container for holding resource

            //    // add part1 to split task
            //    task.Complete = 0.0f; // reset the complete status
            //    var parts = splitTasks[task] = new List<GanttTask>(2);
            //    parts.Add(part1);
            //    splitTaskParents[part1] = task; // make a reverse lookup

            //    if (duration > task.Duration)
            //        duration = task.Duration;

            //    part1.Start = task.Start;
            //    part1.End = task.End;
            //    part1.Duration = task.Duration;

            //    // split part1 to give part2
            //    this.Split(part1, part2, duration);
            //}
        }

        public void Split(GanttTask part, GanttTask other, TimeSpan duration)
        {
            //if (part == null || other == null || registerTask.Contains(other))
            //    return;
            //if (IsPart(part))
            //{
            //    registerTask.Add(other); // register other part
            //    resources[other] = new HashSet<Object>(); // create container for holding resource

            //    var split = splitTaskParents[part]; // get the split task
            //    var parts = splitTasks[split]; // get the list of ordered parts

            //    parts.Insert(parts.IndexOf(part) + 1, other); // insert the other part after the existing part
            //    splitTaskParents[other] = split; // set the reverse lookup

            //    if (duration > part.Duration)
            //        duration = part.Duration;

            //    // the real split
            //    var one_duration = duration;
            //    var two_duration = part.Duration - duration;
            //    part.Duration = one_duration;
            //    part.End = part.Start + one_duration;
            //    other.Duration = two_duration;
            //    other.Start = part.End;
            //    other.End = other.Start + two_duration;

            //    PackPartsForward(parts);
            //    split.Start = parts.First().Start; // recalculate the split task
            //    split.End = parts.Last().End;
            //    split.Duration = split.End - split.Start;

            //    RecalculateSuccessorsOf(split);
            //    RecalculateChildrenSchedule();
            //}
        }

        public void Join(GanttTask part1, GanttTask part2)
        {
            //if (part1 != null
            //    && part2 != null
            //    && splitTaskParents.ContainsKey(part1) // part1 and part2 must already be existing parts
            //    && splitTaskParents.ContainsKey(part2)
            //    && splitTaskParents[part1] == splitTaskParents[part2] // part1 and part2 must be of the same split task
            //    )
            //{

            //    var split = splitTaskParents[part1];
            //    var parts = splitTasks[split];
            //    if (parts.Count > 2)
            //    {
            //        // Aggregate part2 into part1, and determine join type
            //        DateTime min;
            //        bool join_backwards;
            //        if (part1.Start < part2.Start)
            //        {
            //            min = part1.Start;
            //            join_backwards = true;
            //        }
            //        else
            //        {
            //            min = part2.Start;
            //            join_backwards = false;
            //        }
            //        var duration = part1.Duration + part2.Duration;

            //        part1.Start = min;
            //        part1.Duration = duration;
            //        part1.End = min + duration;

            //        // aggregate resouces
            //        // TODO: Ask whether to aggregate resources?
            //        foreach (var r in this.ResourcesOf(part2))
            //            this.Assign(part1, r);
            //        this.Unassign(part2);

            //        // remove all traces of part2
            //        parts.Remove(part2);
            //        resources.Remove(part2);
            //        splitTaskParents.Remove(part2);
            //        registerTask.Remove(part2);

            //        // pack the remaining parts
            //        if (join_backwards) PackPartsForward(parts);
            //        else PackPartsBackwards(parts);

            //        // set the duration
            //        split.End = parts.Last().End;
            //        split.Duration = split.End - split.Start;
            //        split.Start = parts.First().Start;

            //        RecalcChildrenSchedule();
            //    }
            //    else
            //    {
            //        this.Merge(split);
            //    }
            //}
        }
        public void Merge()//合并所有的part
        {
            if (!HasParts)
                return;
            parts[0].RecalcWholeTaskTime();
            //resources.AddRange(parts.SelectMany(p => p.resources));//将所有part的资源重新分配
        }
        public void RecalcChildrenSchedule()
        {
            children.Where(c => c.HasChildren).ForEach(c => c.RecalcChildrenSchedule());
            var duration = children.Sum(c => c.Duration.TotalHours);
            var completed = children.Sum(c => c.Duration.TotalHours * c.Complete);
            this.Complete = completed / duration;
            this.Start = children.MinOrDefault(c => c.Start, Start);
            this.End = children.MaxOrDefault(c => c.End, End);
        }
        void RecalculateSlack()//重新计算松弛
        {
            var minEnd = successorTask.MinOrDefault(t => t.Start, this.End);
            this.Slack = minEnd - this.End;
        }
        void RecalcWholeTaskTime()
        {
            wholeTask.start = wholeTask.parts.First().Start;
            wholeTask.end = wholeTask.parts.Last().End;
            wholeTask.duration = wholeTask.End - wholeTask.Start;
        }
        void SetPartStart(DateTime value)
        {
            value = value.Concat(wholeTask.predecessorTask.Select(t => t.End)).Max();

            var off = value - start;
            start += off;
            end += off;

            if (off < TimeSpan.Zero)
                wholeTask.PackPartsBackwards();
            else
                wholeTask.PackPartsForward();
            RecalcWholeTaskTime();
        }

        void PackPartsBackwards()
        {
            //开始时间不能改变，因此不改变最开始的part
            for (int i = parts.Count - 2; i > 0; i--) // Cannot pack beyond first part (i > 0)
            {
                var current = parts[i];
                var later = parts[i + 1];
                if (later.start <= current.end)
                {
                    current.end = later.start;
                    current.start = current.end - current.duration;
                }
            }
            PackPartsForward();
        }

        void PackPartsForward()
        {
            for (var i = 1; i < parts.Count; i++)
            {
                var prev = parts[i - 1];
                var current = parts[i];
                if (prev.end >= current.start)
                {
                    current.start = prev.end;
                    current.end = current.start + current.duration;
                }
            }
        }

        public void RecalcCompleted()
        {
            if (HasParts)
            {
                var c = parts.Sum(p => p.Complete * p.Duration.TotalHours);
                var d = parts.Sum(p => p.Duration.TotalHours);
                complete = c / d;
            }
            else if (HasChildren)
            {
                children.ForEach(t => t.RecalcCompleted());
                var c = children.Sum(p => p.Complete * p.Duration.TotalHours);
                var d = children.Sum(p => p.Duration.TotalHours);
                complete = c / d;
            }
        }

        void RecalcSuccessorStart()//重新计算后继结点的开始时间
        {
            successorTask.Where(d => d.start <= end).ForEach(t => t.Start = end);
        }

        public int Compare(GanttTask x, GanttTask y)
        {
            throw new NotImplementedException();
        }

        #region IComparable<GanttTask> Members

        public int CompareTo(GanttTask other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
