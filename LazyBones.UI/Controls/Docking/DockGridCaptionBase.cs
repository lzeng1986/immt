using System.Windows.Forms;
using System.Security.Permissions;
using LazyBones.Win32;
using System.Drawing;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Docking
{
    public abstract class DockGridCaptionBase : Control
    {
        protected internal DockGridCaptionBase(DockGrid dockGrid)
        {
            ParamGuard.NotNull(dockGrid, "dockGrid");
            this.dockGrid = dockGrid;
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();
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

        protected bool HasTabPageContextMenu
        {
            get { return dockGrid.HasTabPageContextMenu; }
        }

        protected void ShowTabPageContextMenu(Point position)
        {
            dockGrid.ShowTabPageContextMenu(this, position);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
                ShowTabPageContextMenu(e.Location);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left &&
                dockGrid.DockPanel.AllowEndUserDocking &&
                dockGrid.AllowDockDragAndDrop &&
                !dockGrid.IsAutoHide &&
                dockGrid.ActiveContent != null)
                dockGrid.DockPanel.BeginDrag(dockGrid);
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (dockGrid.IsAutoHide)
                {
                    dockGrid.DockPanel.ActiveAutoHideContent = null;
                    return;
                }

                if (dockGrid.IsFloat)
                    dockGrid.RestoreToPanel();
                else
                    dockGrid.DockStyle = DockStyle.None;
            }
        }

        internal void RefreshChanges()
        {
            if (IsDisposed)
                return;
            OnRefreshChanges();
        }

        protected virtual void OnRightToLeftLayoutChanged()
        {
        }
        protected virtual void OnRefreshChanges()
        {
        }
        protected internal abstract int CaptionHeight { get; }
    }
}
