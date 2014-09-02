using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LazyBones.UI.Controls.Docking
{
    public abstract class AutoHideStripBase : Control
    {
        protected AutoHideStripBase(DockPanel panel)
        {
            dockPanel = panel;
            GridsOnTop = new AutoHideGridCollection(panel, DockStyle.Top);
            GridsOnBottom = new AutoHideGridCollection(panel, DockStyle.Bottom);
            GridsOnLeft = new AutoHideGridCollection(panel, DockStyle.Left);
            GridsOnRight = new AutoHideGridCollection(panel, DockStyle.Right);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        private DockPanel dockPanel;
        protected DockPanel DockPanel
        {
            get { return dockPanel; }
        }

        protected AutoHideGridCollection GridsOnTop { get; private set; }
        protected AutoHideGridCollection GridsOnBottom { get; private set; }
        protected AutoHideGridCollection GridsOnLeft { get; private set; }
        protected AutoHideGridCollection GridsOnRight { get; private set; }

        internal protected AutoHideGridCollection GetGrids(DockStyle dockStyle)
        {
            if (dockStyle == DockStyle.Top)
                return GridsOnTop;
            if (dockStyle == DockStyle.Bottom)
                return GridsOnBottom;
            if (dockStyle == DockStyle.Left)
                return GridsOnLeft;
            if (dockStyle == DockStyle.Right)
                return GridsOnRight;
            throw new ArgumentOutOfRangeException("dockState");
        }
        internal protected Rectangle GetTabStripRectangle(DockStyle dockStyle)
        {
            int height = StripHeight;
            if (dockStyle == DockStyle.Top && GridsOnTop.Any())
            {
                var rect = new Rectangle(0, 0, Width, height);
                if (GridsOnLeft.Any())
                {
                    rect.X = height;
                    rect.Width -= height;
                }
                if (GridsOnRight.Any())
                    rect.Width -= height;
                return rect;
            }
            if (dockStyle == DockStyle.Bottom && GridsOnBottom.Any())
            {
                var rect = new Rectangle(0, Height - height, Width, height);
                if (GridsOnLeft.Any())
                {
                    rect.X = height;
                    rect.Width -= height;
                }
                if (GridsOnRight.Any())
                    rect.Width -= height;
                return rect;
            }
            if (dockStyle == DockStyle.Left && GridsOnLeft.Any())
            {
                return new Rectangle(0, 0, height, Height);
            }
            if (dockStyle == DockStyle.Right && GridsOnRight.Any())
            {
                return new Rectangle(Width - height, 0, height, Height);
            }
            return Rectangle.Empty;
        }

        GraphicsPath displayingArea = new GraphicsPath();

        void SetRegion()
        {
            displayingArea.Reset();
            displayingArea.AddRectangle(GetTabStripRectangle(DockStyle.Top));
            displayingArea.AddRectangle(GetTabStripRectangle(DockStyle.Bottom));
            displayingArea.AddRectangle(GetTabStripRectangle(DockStyle.Left));
            displayingArea.AddRectangle(GetTabStripRectangle(DockStyle.Right));
            Region = new Region(displayingArea);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            var content = HitTest();
            if (content == null)
                return;

            ActiveAutoHideContent = content;

            content.Handler.Activate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            if (!DockPanel.ShowAutoHideContentOnHover)
                return;

            ActiveAutoHideContent = HitTest();

            // requires further tracking of mouse hover behavior,
            ResetMouseEventArgs();
        }
        IDockContent ActiveAutoHideContent
        {
            get { return DockPanel.ActiveAutoHideContent; }
            set
            {
                if (value != null && DockPanel.ActiveAutoHideContent != value)
                    DockPanel.ActiveAutoHideContent = value;
            }
        }
        protected override void OnLayout(LayoutEventArgs levent)
        {
            RefreshChanges();
            base.OnLayout(levent);
        }

        internal void RefreshChanges()
        {
            if (IsDisposed)
                return;
            SetRegion();
            OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int StripHeight { get; }

        internal protected virtual StripTab CreateTab(IDockContent content)
        {
            return new StripTab(content);
        }
        internal protected virtual AutoHideGrid CreateGrid(DockGrid dockGrid)
        {
            return new AutoHideGrid(dockGrid);
        }
        IDockContent HitTest()
        {
            Point ptMouse = PointToClient(Cursor.Position);
            return HitTest(ptMouse);
        }
        protected abstract IDockContent HitTest(Point point);
    }
}
