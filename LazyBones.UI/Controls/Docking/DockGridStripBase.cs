using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Docking
{
    public abstract class DockGridStripBase : Control
    {
        protected DockGridStripBase(DockGrid dockGrid)
        {
            ParamGuard.NotNull(dockGrid, "dockGrid");
            this.dockGrid = dockGrid;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            AllowDrop = true;
        }

        private DockGrid dockGrid;
        protected DockGrid DockGrid
        {
            get { return dockGrid; }
        }
        protected AppearanceStyle Appearance
        {
            get { return dockGrid.Appearance; }
        }
        private DockGridTabCollection tabs = null;
        protected DockGridTabCollection Tabs
        {
            get
            {
                if (tabs == null)
                    tabs = new DockGridTabCollection(dockGrid);
                return tabs;
            }
        }

        internal void RefreshChanges()
        {
            if (IsDisposed)
                return;

            OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }

        protected internal abstract int StripHeight { get; }

        protected internal abstract void EnsureTabVisible(IDockContent content);

        protected int HitTest()
        {
            return HitTest(Cursor.Position);
        }

        protected internal abstract int HitTest(Point screenPoint);

        protected internal abstract GraphicsPath GetOutline(int index);

        protected internal virtual StripTab CreateTab(IDockContent content)
        {
            return new StripTab(content);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                if (dockGrid.DockPanel.AllowEndUserDocking && dockGrid.AllowDockDragAndDrop && dockGrid.ActiveContent.Handler.AllowEndUserDocking)
                    dockGrid.DockPanel.BeginDrag(dockGrid.ActiveContent.Handler);
            }
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = HitTest();
            if (index != -1)
            {
                var content = Tabs[index].Content;
                if (e.Button == MouseButtons.Middle)//点击鼠标中键，关闭当前Content
                {
                    dockGrid.CloseContent(content);
                }
                else if (e.Button == MouseButtons.Left)//点击鼠标左键，激活当前Content
                {
                    if (dockGrid.ActiveContent != content)
                        dockGrid.ActiveContent = content;
                }
                else if (e.Button == MouseButtons.Right)//点击鼠标右键，显示ContextMenu
                {
                    ShowTabPageContextMenu(e.Location);
                }
            }
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int index = HitTest();
            //双击左键，将Content停靠方式转换为Float
            if (e.Button == MouseButtons.Left && index != -1 && dockGrid.DockPanel.AllowEndUserDocking)
            {
                var content = Tabs[index].Content;
                if (content.Handler.IsDockValid(DockStyle.None))
                    content.Handler.DockStyle = DockStyle.None;
            }
        }
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            int index = HitTest();
            if (index != -1)
            {
                IDockContent content = Tabs[index].Content;
                if (dockGrid.ActiveContent != content)
                    dockGrid.ActiveContent = content;
            }
        }
        protected bool HasTabPageContextMenu
        {
            get { return dockGrid.HasTabPageContextMenu; }
        }
        protected void ShowTabPageContextMenu(Point position)
        {
            dockGrid.ShowTabPageContextMenu(this, position);
        }
    }
}
