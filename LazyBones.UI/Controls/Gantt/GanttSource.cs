using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Linq;

namespace LazyBones.UI.Controls.Gantt
{
    public class GanttSource
    {
        SmartList<GanttItem> registerItem = new SmartList<GanttItem>();
        List<GanttItem> rootItems = new List<GanttItem>();
        Dictionary<GanttItem, HashSet<GanttItem>> relations = new Dictionary<GanttItem, HashSet<GanttItem>>();
        Dictionary<GanttItem, HashSet<Object>> resources = new Dictionary<GanttItem, HashSet<Object>>();
        Dictionary<GanttItem, int> itemIndices = new Dictionary<GanttItem, int>();

        public void Add(GanttItem item)
        {
            if (!registerItem.Contains(item))
            {
                registerItem.Add(item);
                rootItems.Add(item);
            }
        }

        public void Remove(GanttItem item)
        {
            if (item == null)
                return;
            if (item.IsPart)
            {
                var splitParent = item.SplitParent;
                var parts = item.SplitParent.Parts;
                if (parts.Count == 1)
                {
                    Merge(splitParent);
                }
                else
                {
                    parts.Remove(item); // remove the part from the split item
                    registerItem.Remove(item); // unregister the part
                    resources.Remove(item);
                    item.SplitParent = null; // remove the reverse lookup

                    splitParent.SetTime(); // recalculate the split item
                }
            }
            else
            {
                if (item.HasChild)
                    ReassginChildren(item);

                if (item.HasPart)
                    Merge(item);

                rootItems.Remove(item);
                item.Children.Clear();
                item.Parts.Clear();
                item.Parent = null;
                item.SplitParent = null;
                relations.Remove(item);
                resources.Remove(item);

                item.Parent.Children.Remove(item);
                foreach (var g in relations.Values)
                    g.Remove(item);
                registerItem.Remove(item);
            }
        }
        public void Group(GanttItem parent, GanttItem child)
        {
            if (parent == null || child == null || parent == child)
                return;
            if (registerItem.Contains(parent))
            {
                child = child.SplitParent ?? child;

                if (registerItem.Contains(child)
                    && !parent.HasPart
                    && !parent.IsPart
                    && !HasRelations(parent)
                    && !AllChildrenOf(child).Contains(parent)
                    )
                {
                    LeaveParent(child);
                    parent.Children.Add(child);
                    child.Parent = parent;
                    RecalcChildrenSchedule(parent);
                }
            }
        }

        public void Ungroup(GanttItem parent, GanttItem child)
        {
            if (parent == null || child == null || parent == child)
                return;
            if (registerItem.Contains(parent))
            {
                child = child.SplitParent ?? child;

                if (registerItem.Contains(child) && parent.HasChild)
                {
                    var root = AllParentsOf(parent).LastOrDefault() ?? parent;
                    rootItems.Insert(rootItems.IndexOf(root) + 1, child);
                    parent.Children.Remove(child);
                    child.Parent = null;
                    RecalcChildrenSchedule(parent);
                }
            }
        }
        public void AddTo(GanttItem item, GanttItem part)
        {
            AddTo(item, part, -1);
        }
        public void AddTo(GanttItem item, GanttItem part, int index)
        {
            if (item == null || part == null || !registerItem.Contains(item))
                return;
            if (item.IsPart || item.HasChild || registerItem.Contains(part))
                return;
            AddInternal(item, part, index);
        }
        public void AddTo(GanttItem item, GanttItem part, GanttItem before)
        {
            if (item == null || part == null || before == null || !registerItem.Contains(item) || before.SplitParent != item)
                return;
            if (item.IsPart || item.HasChild || registerItem.Contains(part))
                return;
            AddInternal(item, part, item.Parts.IndexOf(before));
        }
        void AddInternal(GanttItem item, GanttItem part, int index)
        {
            registerItem.Add(part);  // register part1
            if (index == -1)
                item.Parts.Add(part);
            else
                item.Parts.Insert(index, part);
            part.SplitParent = item;
            item.SetTime();
            RecalcSuccessorStart(item);
        }

        public void Join(GanttItem part1, GanttItem part2)//将part2合并到part1
        {
            if (part1 != null
                && part2 != null
                && part1.SplitParent != null
                && part1.SplitParent == part2.SplitParent // part1 and part2 must be of the same split item
                )
            {
                var split = part1.SplitParent;
                var parts = split.Parts;
                if (parts.Count > 2)
                {
                    // Aggregate part2 into part1, and determine join type
                    DateTime min;
                    bool join_backwards;
                    if (part1.Start < part2.Start)
                    {
                        min = part1.Start;
                        join_backwards = true;
                    }
                    else
                    {
                        min = part2.Start;
                        join_backwards = false;
                    }
                    var duration = part1.Duration + part2.Duration;

                    part1.Start = min;
                    part1.Duration = duration;
                    part1.End = min + duration;

                    // aggregate resouces
                    // TODO: Ask whether to aggregate resources?
                    foreach (var r in this.ResourcesOf(part2))
                        this.Assign(part1, r);
                    this.Unassign(part2);

                    // remove all traces of part2
                    parts.Remove(part2);
                    resources.Remove(part2);
                    part2.SplitParent = null;
                    registerItem.Remove(part2);

                    // pack the remaining parts
                    if (join_backwards)
                        PackPartsForward(parts);
                    else
                        PackPartsBackwards(parts);

                    split.SetTime();
                    RecalcSuccessorStart(split);
                }
                else
                {
                    this.Merge(split);
                }
            }
        }

        public void Merge(GanttItem item)//合并所有part
        {
            if (item == null)
                return;
            var duration = item.Parts.Aggregate(TimeSpan.Zero, (span, t) => span + t.Duration);
            Assign(item, item.Parts.SelectMany(ResourcesOf));//将所有part的资源重新分配给split
            item.Parts.ForEach(t =>
            {
                t.SplitParent = null;
                registerItem.Remove(t);
                resources.Remove(t);
            });
            item.Parts.Clear();
            item.Parts.Clear();
            SetDuration(item, duration);
        }

        public void ReassginChildren(GanttItem parent)//将子节点中心分配
        {
            if (parent == null)
                return;
            if (parent.Parent == null)
            {
                rootItems.AddRange(parent.Children);
            }
            else
            {
                parent.Parent.Children.AddRange(parent.Children);
            }
            parent.Children.ForEach(m => m.Parent = parent.Parent);
            parent.Children.Clear();
            RecalcChildrenSchedule();
        }

        public void Move(GanttItem item, int offset)
        {
            if (item != null && registerItem.Contains(item) && offset != 0)
            {
                int indexofitem = IndexOf(item);
                if (indexofitem > -1)
                {
                    int newindexofitem = indexofitem + offset;
                    // check for out of index bounds
                    if (newindexofitem < 0)
                        newindexofitem = 0;
                    else if (newindexofitem > Items.Count())
                        newindexofitem = Items.Count();
                    // get the index of the item that will be displaced
                    var displaceditem = Items.ElementAtOrDefault(newindexofitem);

                    if (displaceditem == null)
                    {
                        LeaveParent(item);
                        rootItems.Add(item);
                        itemIndices.Clear();
                    }
                    else if (!displaceditem.Equals(item))
                    {
                        int indexofdestinationitem;
                        var displaceditemparent = displaceditem.Parent;
                        if (displaceditemparent == null) // displaceditem is in root
                        {
                            indexofdestinationitem = rootItems.IndexOf(displaceditem);
                            LeaveParent(item);
                            rootItems.Insert(indexofdestinationitem, item);
                        }
                        else if (!displaceditemparent.Equals(item)) // displaced item is not under the moving item
                        {
                            var memberlist = displaceditemparent.Children;
                            indexofdestinationitem = memberlist.IndexOf(displaceditem);
                            LeaveParent(item);
                            memberlist.Insert(indexofdestinationitem, item);
                            item.Parent = displaceditemparent;
                        }

                        // clear indices since positions changed
                        itemIndices.Clear();
                    }
                    else // displaceditem == item, no need to move    
                    {

                    }
                }
            }
        }

        public void Relate(GanttItem from, GanttItem to)
        {
            if (registerItem.Contains(from) && registerItem.Contains(to))
            {
                from = from.SplitParent ?? from;
                to = to.SplitParent ?? to;

                if (from != to
                    && !from.HasChild
                    && !to.HasChild
                    && !this.PredecessorsOf(to).Contains(from)
                    )
                {
                    if (!relations.ContainsKey(from))
                        relations.Add(from, new HashSet<GanttItem>());
                    relations[from].Add(to);
                    RecalcSuccessorStart(from);
                    RecalcSlack(from);
                }
            }
        }

        public void Unrelate(GanttItem from, GanttItem to)
        {
            if (registerItem.Contains(from) && registerItem.Contains(to))
            {
                from = from.SplitParent ?? from;
                to = to.SplitParent ?? to;
                relations[from].Remove(to);
                if (relations[from].Count == 0)
                    relations.Remove(from);
                RecalcSlack(from);
            }
        }

        public void Unrelate(GanttItem item)
        {
            if (registerItem.Contains(item))
            {
                item = item.SplitParent ?? item;
                relations[item].Clear();
                relations.Remove(item);
                RecalcSlack();
            }
        }

        public IEnumerable<GanttItem> ItemsHasRelation
        {
            get { return relations.Keys; }
        }

        public IEnumerable<GanttItem> Items
        {
            get { return rootItems.DFS(i => i.Children); }
        }
        public IEnumerable<GanttItem> VisibleItems
        {
            get
            {
                return rootItems.DFS(t => t.IsCollapsed ? Enumerable.Empty<GanttItem>() : t.Children)
                .Where(t => t.Start != DateTime.MinValue && t.Duration != TimeSpan.Zero);
            }
        }
        public IEnumerable<GanttItem> RootItems
        {
            get { return rootItems; }
        }
        public void SetStart(GanttItem item, DateTime start)
        {
            if (registerItem.Contains(item) && start != item.Start && !item.HasChild)
            {
                SetStartHelper(item, start);
                RecalcChildrenSchedule(item);
                RecalcSuccessorStart(item);
            }
        }

        public void SetEnd(GanttItem item, DateTime end)
        {
            if (registerItem.Contains(item) && end != item.End && !item.HasChild)
            {
                SetEndHelper(item, end);
                RecalcChildrenSchedule(item);
                RecalcSuccessorStart(item);
            }
        }
        public void SetDuration(GanttItem item, double hours)
        {
            SetDuration(item, TimeSpan.FromHours(hours));
        }
        public void SetDuration(GanttItem item, TimeSpan duration)
        {
            SetEnd(item, item.Start + duration);
        }

        public void SetComplete(GanttItem item, float complete)
        {
            if (registerItem.Contains(item)
                && complete != item.Complete
                && !item.HasChild // not a group
                && !item.HasPart // not a split item
                )
            {
                SetCompleteHelper(item, complete);
                RecalculateComplete();
            }
        }

        public void SetCollapse(GanttItem item, bool collasped)
        {
            if (registerItem.Contains(item) && item.HasChild)
            {
                item.IsCollapsed = collasped;
            }
        }

        public int IndexOf(GanttItem item)
        {
            if (registerItem.Contains(item))
            {
                int ind;
                if (itemIndices.TryGetValue(item, out ind))
                    return ind;
                else
                {
                    ind = Items.IndexOf(item);
                    itemIndices[item] = ind;
                    return ind;
                }
            }
            return -1;
        }

        public IEnumerable<GanttItem> AllParentsOf(GanttItem item)//返回所有的父节点
        {
            while (item.Parent != null)
            {
                yield return item.Parent;
                item = item.Parent;
            }
        }

        public IEnumerable<GanttItem> AllChildrenOf(GanttItem item)//返回所有的子节点，广度有限搜索
        {
            return item.Children.BFS(i => i.Children);
        }

        public IEnumerable<GanttItem> PredecessorsOf(GanttItem item)
        {
            return DirectPredecessorsOf(item).BFS(DirectPredecessorsOf);
        }

        public IEnumerable<GanttItem> SuccessorsOf(GanttItem item)
        {
            return DirectSuccessorsOf(item).BFS(DirectSuccessorsOf);
        }

        public IEnumerable<GanttItem> DirectPredecessorsOf(GanttItem item)
        {
            return relations.Where(x => x.Value.Contains(item)).Select(x => x.Key);
        }

        public IEnumerable<GanttItem> DirectSuccessorsOf(GanttItem item)
        {
            if (item == null)
                yield break;

            HashSet<GanttItem> set;
            if (relations.TryGetValue(item, out set))
            {
                foreach (var v in set)
                    yield return v;
            }
        }

        public IEnumerable<IEnumerable<GanttItem>> CriticalPaths
        {
            get
            {
                if (Items.Any())
                {
                    foreach (var item in Items.GroupBy(t => t.End).MaxBy(t => t.Key))
                    {
                        yield return new[] { item }.Concat(PredecessorsOf(item));
                    }
                }
            }
        }

        public bool HasRelations(GanttItem item)
        {
            if (registerItem.Contains(item) && relations.ContainsKey(item))
            {
                return relations[item].Count > 0 || DirectPredecessorsOf(item).Any();
            }
            else
            {
                return false;
            }
        }

        public void Assign(GanttItem item, Object resource)//向item分配指定资源
        {
            if (!registerItem.Contains(item))
                resources[item] = new HashSet<object>();
            resources[item].Add(resource);
        }
        public void Assign(GanttItem item, IEnumerable<object> resource)//向item分配指定资源
        {
            if (item == null || resource == null)
                return;
            if (!registerItem.Contains(item))
                resources[item] = new HashSet<object>();
            var host = resources[item];
            resource.ForEach(r => host.Add(r));
        }
        public void Unassign(GanttItem item, Object resource)//清空item的指定资源
        {
            resources[item].Remove(resource);
            if (resources[item].Count == 0)
                resources.Remove(item);
        }

        public void Unassign(GanttItem item)//清空item的资源
        {
            if (registerItem.Contains(item))
            {
                resources[item].Clear();
                resources.Remove(item);
            }
        }

        public void Unassign(Object resource)//删除资源
        {
            foreach (var v in resources.Values)
                v.Remove(resource);
        }

        public IEnumerable<Object> Resources
        {
            get { return resources.SelectMany(x => x.Value).Distinct(); }
        }

        public IEnumerable<Object> ResourcesOf(GanttItem item)
        {
            if (item == null)
                yield break;

            HashSet<Object> list;
            if (resources.TryGetValue(item, out list))
            {
                foreach (var i in list)
                    yield return i;
            }
        }

        public IEnumerable<GanttItem> itemsOf(Object resource)
        {
            return resources.Where(x => x.Value.Contains(resource)).Select(x => x.Key);
        }

        void LeaveParent(GanttItem item)
        {
            if (item.Parent == null)
                rootItems.Remove(item);
            else
            {
                item.Parent.Children.Remove(item);
            }
            item.Parent = null;
        }

        void SetStartHelper(GanttItem item, DateTime value)
        {
            if (item.Start != value)
            {
                if (item.IsPart)//如果item是另一个item的一部分，则调用SetPartStartHelper设置Start
                {
                    value = value.Concat(DirectPredecessorsOf(item.SplitParent).Select(x => x.End)).Max();

                    var offset = value - item.Start;
                    MoveItem(item, offset);

                    if (offset < TimeSpan.Zero)
                        PackPartsBackwards(item.Parts);
                    else
                        PackPartsForward(item.Parts);
                    
                    item.SplitParent.SetTime();
                    RecalcSuccessorStart(item.SplitParent);
                }
                else
                {
                    value = value.Concat(DirectPredecessorsOf(item).Select(t => t.End)).Max();
                    var offset = value - item.Start;
                    MoveItem(item, offset);
                    item.Parts.ForEach(i => MoveItem(i, offset));

                    RecalcSuccessorStart(item);
                }
            }
        }
        static void MoveItem(GanttItem item, TimeSpan offset)
        {
            item.Start += offset;
            item.End += offset;
        }
        void SetEndHelper(GanttItem item, DateTime value)
        {
            if (item.End != value)
            {
                if (item.Start > value)//value不得小于Start
                    value = item.Start;

                if (item.IsPart)
                {
                    var split = item.SplitParent;
                    var parts = split.Parts;

                    var increased = value > item.End;
                    SetEndDirect(item, value);

                    if (increased)
                        PackPartsForward(parts);

                    SetTime(split, parts);

                    RecalcSuccessorStart(split);
                }
                else if (item.HasPart)
                {
                    var lastPart = item.Parts.Last();
                    if (value < lastPart.Start)//value不得小于最后一个part的Start
                        value = lastPart.Start;

                    item.SetEndDirect(value);
                    lastPart.SetEndDirect(value);

                    RecalcSuccessorStart(item);
                }
                else
                {
                    SetEndDirect(item, value);
                }
            }
        }
        static void SetEndDirect(GanttItem item, DateTime value)
        {
            item.End = value;
            item.Duration = item.End - item.Start;
        }
        static void SetTime(GanttItem item, List<GanttItem> parts)
        {
            item.Start = parts.First().Start;
            item.End = parts.Last().End;
            item.Duration = item.End - item.Start;
        }
        void PackPartsBackwards(List<GanttItem> parts)
        {
            //开始时间不能改变，因此不改变最开始的part
            for (int i = parts.Count - 2; i > 0; i--) // Cannot pack beyond first part (i > 0)
            {
                var current = parts[i];
                var later = parts[i + 1];
                if (later.Start <= current.End)
                {
                    current.End = later.Start;
                    current.Start = current.End - current.Duration;
                }
            }
            PackPartsForward(parts);
        }

        void PackPartsForward(List<GanttItem> parts)
        {
            for (var i = 1; i < parts.Count; i++)
            {
                var prev = parts[i - 1];
                var current = parts[i];
                if (prev.End >= current.Start)
                {
                    current.Start = prev.End;
                    current.End = current.Start + current.Duration;
                }
            }
        }

        void SetCompleteHelper(GanttItem item, double value)
        {
            if (item.Complete != value)
            {
                value = Math.Min(Math.Max(0, value), 1);//调整到0~1之间
                item.Complete = value;

                if (item.IsPart)
                {
                    var complete = item.Parts.Sum(p => p.Complete * p.Duration.TotalHours);
                    var duration = item.Parts.Sum(p => p.Duration.TotalHours);
                    item.SplitParent.Complete = complete / duration;
                }
            }
        }

        void RecalculateComplete()
        {
            foreach (var item in rootItems.Where(x => x.HasChild))
            {
                RecalculateCompletedHelper(item);
            }
        }

        void RecalculateCompletedHelper(GanttItem item)
        {
            double complete = 0;
            double duration = 0;

            if (item.HasPart)
            {
                foreach (var part in item.Parts)
                {
                    complete += part.Complete * part.Duration.TotalHours;
                    duration += part.Duration.TotalHours;
                }
            }
            else
            {
                foreach (var member in item.Children)
                {
                    if (member.HasChild)
                        RecalculateCompletedHelper(member);
                    duration += member.Duration.TotalHours;
                    complete += member.Complete * member.Duration.TotalHours;
                }
            }

            item.Complete = complete / duration;
        }

        void RecalcSuccessorStart(GanttItem precedent)//重新计算后继结点的开始时间
        {
            foreach (var dependant in this.DirectSuccessorsOf(precedent).Where(d => d.Start <= precedent.End))
            {
                SetStartHelper(dependant, precedent.End);
            }
        }

        private void RecalcChildrenSchedule()
        {
            foreach (var parent in rootItems.Where(x => x.HasChild))
            {
                RecalcChildrenSchedule(parent);
            }
        }

        private void RecalcChildrenSchedule(GanttItem parent)
        {
            foreach (var child in parent.Children.Where(x => x.HasChild))
                RecalcChildrenSchedule(child);

            var duration = parent.Children.Sum(c => c.Duration.TotalHours);
            var completed = parent.Children.Sum(c => c.Duration.TotalHours * c.Complete);
            var start = parent.Children.MinOrDefault(c => c.Start, parent.Start);
            var end = parent.Children.MaxOrDefault(c => c.End, parent.End);

            SetStartHelper(parent, start);
            SetEndHelper(parent, end);
            SetCompleteHelper(parent, completed / duration);
        }
        void RecalcSlack(GanttItem item)
        {
            var end = DirectSuccessorsOf(item).MinOrDefault(x => x.Start, item.End);
            item.Slack = end - item.End;
        }
        void RecalcSlack()//重新计算松弛
        {
            var maxEnd = this.Items.Max(x => x.End);
            foreach (var item in this.Items)
            {
                var end = DirectSuccessorsOf(item).MinOrDefault(x => x.Start, maxEnd);
                item.Slack = end - item.End;
            }
        }
    }
}
