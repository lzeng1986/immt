using System.Drawing;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 处理停靠的拖拽
    /// </summary>
    internal class DockDragHandler : DragHandler<IDockDragSource>
    {
        DockIndicator dockIndicator;
        Rectangle floatOutlineBounds;
        public DockDragHandler(DockPanel dockPanel)
            : base(dockPanel)
        {
            Outline = new DockOutline();
        }
        public DockOutline Outline { get; private set; }

        public void BeginDrag(IDockDragSource dragSource)
        {
            DragSource = dragSource;

            if (!BeginDrag())
            {
                DragSource = null;
                return;
            }

            Outline = new DockOutline();
            dockIndicator = new DockIndicator(this);
            dockIndicator.Show();

            floatOutlineBounds = DragSource.BeginDrag(StartMousePosition);
        }

        protected override void OnDragging()
        {
            TestDrop();
        }

        protected override void OnEndDrag(bool abort)
        {
            DockPanel.SuspendLayout();

            Outline.Close();
            dockIndicator.Close();
            dockIndicator = null;

            EndDrag(abort);

            DockPanel.ResumeLayout(true);

            DragSource.EndDrag();

            DragSource = null;
        }

        void TestDrop()
        {
            Outline.FlagTestDrop = false;

            dockIndicator.FullPanelEdge = Control.ModifierKeys.HasFlag(Keys.Shift);

            if (dockIndicator.FullPanelEdge)
            {
                dockIndicator.TestDrop();

                if (!Outline.FlagTestDrop)
                {
                    var grid = DockPanel.GetGridAtCursor();
                    if (grid != null && DragSource.IsDockValid(grid.DockStyle))
                        grid.TestDrop(DragSource, Outline);
                }

                if (!Outline.FlagTestDrop && DragSource.IsDockValid(DockStyle.None))
                {
                    var floatWindow = DockPanel.GetFloatWindowAtCursor();
                    if (floatWindow != null)
                        floatWindow.TestDrop(DragSource, Outline);
                }
            }
            else
                dockIndicator.DockGrid = DockPanel.GetGridAtCursor();

            if (!Outline.FlagTestDrop)
            {
                if (DragSource.IsDockValid(DockStyle.None))
                {
                    var rect = floatOutlineBounds;
                    var pt = Cursor.Position - (Size)StartMousePosition;
                    rect.Offset(pt);
                    Outline.Show(rect);
                }
            }

            if (!Outline.FlagTestDrop)
            {
                Cursor.Current = Cursors.No;
                Outline.Show();
            }
            else
                Cursor.Current = DragControl.Cursor;
        }

        void EndDrag(bool abort)
        {
            if (abort)
                return;

            if (!Outline.FloatWindowBounds.IsEmpty)
                DragSource.FloatAt(Outline.FloatWindowBounds);
            else if (Outline.DockTo is DockGrid)
            {
                var grid = Outline.DockTo as DockGrid;
                DragSource.DockTo(grid, Outline.DockStyle, Outline.ContentIndex);
            }
            else if (Outline.DockTo is DockPanel)
            {
                DockPanel panel = Outline.DockTo as DockPanel;
                panel.UpdateDockWindowZOrder(Outline.DockStyle, Outline.FlagFullEdge);
                DragSource.DockTo(panel, Outline.DockStyle);
            }
        }

    }
}
