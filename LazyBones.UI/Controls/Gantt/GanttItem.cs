using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Drawing;

namespace LazyBones.UI.Controls.Gantt
{
    public class GanttItem : IComparable<GanttItem>
    {
        public GanttItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            Name = name;
            Start = DateTime.MinValue;
            End = DateTime.MinValue;
            Duration = TimeSpan.Zero;
            Slack = TimeSpan.Zero;
        }
        internal RectangleF DrawRect;
        internal RectangleF SlackRect;
        internal GanttItem Parent;
        internal List<GanttItem> Children = new List<GanttItem>();
        internal GanttItem SplitParent;
        internal List<GanttItem> Parts = new List<GanttItem>();

        public bool IsPart
        {
            get { return SplitParent != null; }
        }
        public bool IsChild
        {
            get { return Parent != null; }
        }
        public bool HasPart
        {
            get { return Parts.Count > 0; }
        }
        public bool HasChild
        {
            get { return Children.Count > 0; }
        }

        internal void SetEndDirect(DateTime value)
        {
            End = value;
            Duration = End - Start;
        }
        internal void SetTime()
        {
            Start = Parts.First().Start;
            End = Parts.Last().End;
            Duration = End - Start;
        }
        internal void RemoveFromParent()
        {
            if (Parent != null)
                Parent.Children.Remove(this);
        }
        public string Name { get; private set; }
        public string ToolTip { get; set; }
        public bool IsCollapsed { get; set; }

        public double Complete { get; internal set; }

        public DateTime Start { get; internal set; }

        public DateTime End { get; internal set; }

        public TimeSpan Duration { get; internal set; }

        public TimeSpan Slack { get; internal set; }

        public int CompareTo(GanttItem other)
        {
            if (other == null)
                return 1;
            return Name.CompareTo(other.Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
