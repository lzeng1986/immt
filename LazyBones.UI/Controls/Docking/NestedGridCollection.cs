using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LazyBones.Utils;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 停靠在DockWindow或者FloatWindow内部的DockGrid的集合
    /// </summary>
    public class NestedGridCollection : IEnumerable<DockGrid>
    {
        List<DockGrid> dockGrids = new List<DockGrid>();
        internal NestedGridCollection(INestedGridsContainer container)
        {
            ParamGuard.NotNull(container, "container");
            Container = container;
        }
        public INestedGridsContainer Container { get; private set; }
        public int IndexOf(DockGrid item)
        {
            return dockGrids.IndexOf(item);
        }

        public DockGrid this[int index]
        {
            get { return dockGrids[index]; }
        }
        public void Add(DockGrid item)
        {
            Insert(item, -1);
        }
        public void Insert(DockGrid item, DockGrid prevGrid)
        {
            Insert(item, dockGrids.IndexOf(prevGrid));
        }
        public void Insert(DockGrid item, int index)//从原来Container中删除Grid，并重新计算所有Grid的边界
        {
            if (item == null)
                return;
            if (item.Container != null && item.Container.NestedGrids != null)
                item.Container.NestedGrids.Remove(item);
            if (index == -1)
                dockGrids.Add(item);
            else
                dockGrids.Insert(index, item);
            CalculateGridBounds();
        }
        public void Clear()
        {
            dockGrids.Clear();
            OnDockGridRemoved();
        }

        public bool Contains(DockGrid item)
        {
            return dockGrids.Contains(item);
        }

        public int Count
        {
            get { return dockGrids.Count; }
        }

        public bool Remove(DockGrid item)
        {
            if (dockGrids.Remove(item))
            {
                CalculateGridBounds();
                OnDockGridRemoved();
                return true;
            }
            return false;
        }

        public IEnumerator<DockGrid> GetEnumerator()
        {
            return dockGrids.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        internal void Refresh()
        {
            CalculateGridBounds();
        }
        void CalculateGridBounds()//计算每个内部Grid的边界范围
        {
            if (dockGrids.Count == 0)
                return;

            dockGrids[0].Bounds = Container.DisplayRectangle;
            dockGrids[0].SplitterBounds = Rectangle.Empty;

            for (var i = 1; i < dockGrids.Count; i++)
            {
                var grid = dockGrids[i];
                var prevGrid = dockGrids[i - 1];
                var rect = prevGrid.Bounds;
                grid.Bounds = prevGrid.Bounds;
                switch (grid.NestedDockStyle)
                {
                    case DockStyle.Left:
                        grid.Width = (int)(rect.Width * grid.NestedProportion);
                        prevGrid.Left = grid.Right;
                        prevGrid.Width = rect.Width - grid.Width;
                        break;
                    case DockStyle.Right:
                        grid.Left = (int)(rect.Width * (1 - grid.NestedProportion));
                        grid.Width = (int)(rect.Width * grid.NestedProportion);
                        prevGrid.Width = rect.Width - grid.Width;
                        break;
                    case DockStyle.Top:
                        grid.Height = (int)(rect.Height * grid.NestedProportion);
                        prevGrid.Top = grid.Bottom;
                        prevGrid.Height = rect.Height - grid.Height;
                        break;
                    case DockStyle.Bottom:
                        grid.Top = (int)(rect.Height * (1 - grid.NestedProportion));
                        grid.Height = (int)(rect.Height * grid.NestedProportion);
                        prevGrid.Height = rect.Height - grid.Height;
                        break;
                }
            }
        }
        internal event EventHandler DockGridRemoved;
        void OnDockGridRemoved()
        {
            DockGridRemoved.SafeCall(this);
        }
    }
}
