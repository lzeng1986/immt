using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    public class AutoHideGrid : IDisposable
    {
        public AutoHideGrid(DockGrid dockGrid)
        {
            DockGrid = dockGrid;
        }
        ~AutoHideGrid()
        {
            Dispose(false);
        }
        public DockGrid DockGrid { get; private set; }
        public AutoHideTabCollection AutoHideTabs
        {
            get { return DockGrid.AutoHideTabs; }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
        }
    }
    class AutoHideStyle
    {
        public AutoHideStyle(DockStyle dockStyle)
        {
            DockStyle = dockStyle;
            Selected = false;
        }
        public DockStyle DockStyle { get; private set; }
        public bool Selected { get; set; }
    }
    class AutoHideStateCollection
    {
        AutoHideStyle[] states = new[]
        {
            new AutoHideStyle(DockStyle.Left),
            new AutoHideStyle(DockStyle.Top),
            new AutoHideStyle(DockStyle.Right),
            new AutoHideStyle(DockStyle.Bottom)
        };
        public AutoHideStyle this[DockStyle dockStyle]
        {
            get
            {
                var state = states.FirstOrDefault(s => s.DockStyle == dockStyle);
                if (state != null)
                    return state;
                throw new ArgumentOutOfRangeException("dockState");
            }
        }
        public bool ContainsGrid(DockGrid grid)
        {
            return grid.Visible && states.Any(s => s.DockStyle == grid.DockStyle && s.Selected);
        }
    }
    public sealed class AutoHideGridCollection : IEnumerable<AutoHideGrid>
    {
        AutoHideStateCollection states = new AutoHideStateCollection();
        internal AutoHideGridCollection(DockPanel panel, DockStyle dockStyle)
        {
            DockPanel = panel;
            states[dockStyle].Selected = true;
        }
        public DockPanel DockPanel { get; private set; }
        public int Count
        {
            get { return this.Count(); }
        }
        public AutoHideGrid this[int index]
        {
            get { return this.ElementAt(index); }
        }
        public bool Contains(AutoHideGrid grid)
        {
            if (grid == null)
                return false;
            return DockPanel.Grids.Any(g => g.AutoHideGrid == grid && states.ContainsGrid(g));
        }

        public IEnumerator<AutoHideGrid> GetEnumerator()
        {
            return DockPanel.Grids.Where(g => states.ContainsGrid(g))
                .Select(g => g.AutoHideGrid).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
