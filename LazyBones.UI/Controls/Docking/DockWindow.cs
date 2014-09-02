using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 表示停靠窗口，用于承载<see cref="DockGrid"/>，一个<see cref="DockPanel"/>包含左、上、右、下、中间五个<see cref="DockWindow"/>
    /// </summary>
    internal class DockWindow : Panel, ISplitterDragSource, INestedGridsContainer
    {
        Splitter splitter;
        internal DockWindow(DockPanel dockPanel, DockStyle dockStyle)
        {
            if (dockStyle == DockStyle.None)
                throw new ArgumentException("dockStyle值不得为DockStyle.None", "dockStyle");
            NestedGrids = new NestedGridCollection(this);
            DockPanel = dockPanel;
            Dock = dockStyle;
            Visible = false;

            if (dockStyle != DockStyle.Fill || dockStyle != DockStyle.None)//只有左、上、右、下才包含splitter
            {
                SuspendLayout();
                splitter = new Splitter(dockPanel);
                Controls.Add(splitter);
                if (dockStyle == DockStyle.Left)
                    splitter.Dock = DockStyle.Right;
                else if (dockStyle == DockStyle.Right)
                    splitter.Dock = DockStyle.Left;
                else if (dockStyle == DockStyle.Top)
                    splitter.Dock = DockStyle.Bottom;
                else if (dockStyle == DockStyle.Bottom)
                    splitter.Dock = DockStyle.Top;
                ResumeLayout();
            }

            AutoHidePortion = 0.25;
        }
        public NestedGridCollection NestedGrids { get; private set; }
        public DockPanel DockPanel { get; private set; }
        public bool IsFloat { get { return false; } }
        public double AutoHidePortion { get; set; }

        internal DockGrid DefaultGrid
        {
            get { return NestedGrids.FirstOrDefault(); }
        }
        public override Rectangle DisplayRectangle  //ClientRectangle减去splitter的大小
        {
            get
            {
                var rect = ClientRectangle;
                if (Dock == DockStyle.Fill)
                {
                    rect.Inflate(-1, -1);
                }
                else if (Dock == DockStyle.Left)
                    rect.Width -= Const.SplitterSize;
                else if (Dock == DockStyle.Right)
                {
                    rect.X += Const.SplitterSize;
                    rect.Width -= Const.SplitterSize;
                }
                else if (Dock == DockStyle.Top)
                    rect.Height -= Const.SplitterSize;
                else if (Dock == DockStyle.Bottom)
                {
                    rect.Y += Const.SplitterSize;
                    rect.Height -= Const.SplitterSize;
                }
                return rect;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Dock == DockStyle.Fill)    //绘制一个边框
                e.Graphics.DrawRectangle(SystemPens.ControlDark,
                    ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            base.OnPaint(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            NestedGrids.Refresh();
            if (!NestedGrids.Any())
            {
                if (Visible)
                    Visible = false;
            }
            else if (!Visible)
            {
                Visible = true;
                NestedGrids.Refresh();
            }
            base.OnLayout(levent);
        }

        public void BeginDrag(Rectangle splitterScreenBounds)
        {
        }

        public void EndDrag()
        {
        }

        public bool IsVertical
        {
            get { return (Dock == DockStyle.Left || Dock == DockStyle.Right); }
        }

        public Rectangle DragLimitBounds
        {
            get
            {
                var limitRect = DockPanel.DockBounds;
                if (IsVertical)
                    limitRect.Inflate(-Const.MinGridSize, 0);
                else
                    limitRect.Inflate(0, -Const.MinGridSize);
                return DockPanel.RectangleToScreen(limitRect);
            }
        }

        public void MoveSplitter(int offset)
        {
            var rectDockArea = DockPanel.DockBounds;
            if (Dock == DockStyle.Left && rectDockArea.Width > 0)
            {
                if (AutoHidePortion > 1)
                    AutoHidePortion = Width + offset;
                else
                    AutoHidePortion += offset * 1.0 / rectDockArea.Width;
            }
            else if (Dock == DockStyle.Right && rectDockArea.Width > 0)
            {
                if (AutoHidePortion > 1)
                    AutoHidePortion = Width - offset;
                else
                    AutoHidePortion -= ((double)offset) / (double)rectDockArea.Width;
            }
            else if (Dock == DockStyle.Bottom && rectDockArea.Height > 0)
            {
                if (AutoHidePortion > 1)
                    AutoHidePortion = Height - offset;
                else
                    AutoHidePortion -= ((double)offset) / (double)rectDockArea.Height;
            }
            else if (Dock == DockStyle.Top && rectDockArea.Height > 0)
            {
                if (AutoHidePortion > 1)
                    AutoHidePortion = Height + offset;
                else
                    AutoHidePortion += ((double)offset) / (double)rectDockArea.Height;
            }
        }

        public Control DragControl
        {
            get { return this; }
        }
        public DockStyle DockStyle { get; private set; }
    }
}
