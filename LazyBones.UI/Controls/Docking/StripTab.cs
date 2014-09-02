using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Linq;

namespace LazyBones.UI.Controls.Docking
{
    public class StripTab : IDisposable
    {
        public StripTab(IDockContent content)
        {
            Content = content;
        }
        ~StripTab()
        {
            Dispose(false);
        }
        public IDockContent Content { get; private set; }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Content = null;
            }
        }
        public int RequiredWidth { get; set; }
        public int DisplayWidth { get; set; }
        public int Left { get; set; }
    }
    public abstract class StripTabCollection<T> : IEnumerable<T>
        where T : StripTab
    {
        protected StripTabCollection(DockGrid dockGrid)
        {
            this.dockGrid = dockGrid;
        }
        protected DockGrid dockGrid;
        public DockGrid DockGrid
        {
            get { return dockGrid; }
        }
        public DockPanel DockPanel
        {
            get { return dockGrid.DockPanel; }
        }
        public int Count
        {
            get { return dockGrid.DisplayingContents.Count(); }
        }
        public abstract T this[int index] { get; }

        public bool Contains(T tab)
        {
            if (tab == null)
                return false;
            return Contains(tab.Content);
        }

        public bool Contains(IDockContent content)
        {
            return dockGrid.DisplayingContents.Contains(content);
        }

        public int IndexOf(T tab)
        {
            if (tab == null)
                return -1;
            return IndexOf(tab.Content);
        }

        public virtual int IndexOf(IDockContent content)
        {
            if (content == null)
                return -1;
            return dockGrid.DisplayingContents.IndexOf(content);
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class DockGridTabCollection : StripTabCollection<StripTab>
    {
        public DockGridTabCollection(DockGrid grid)
            : base(grid)
        {
        }
        public override StripTab this[int index]
        {
            get
            {
                var content = dockGrid.DisplayingContents.ElementAt(index);
                return (content.Handler.GridStripTab ?? (content.Handler.GridStripTab = dockGrid.TabStrip.CreateTab(content)));
            }
        }
    }
    public class AutoHideTabCollection : StripTabCollection<StripTab>
    {
        public AutoHideTabCollection(DockGrid grid)
            : base(grid)
        {
        }

        public override StripTab this[int index]
        {
            get
            {
                var content = dockGrid.DisplayingContents.ElementAt(index);
                return (content.Handler.AutoHideTab ?? (content.Handler.AutoHideTab = DockPanel.AutoHideStrip.CreateTab(content)));
            }
        }
    }
}
