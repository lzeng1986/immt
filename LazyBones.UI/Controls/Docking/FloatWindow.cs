using System;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using LazyBones.Utils;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 表示浮动窗体，用于承载<see cref="DockGrid"/>
    /// </summary>
    public class FloatWindow : Form, INestedGridsContainer, IDockDragSource
    {
        internal protected FloatWindow(DockPanel dockPanel)
            : this(dockPanel, Rectangle.Empty)
        {
        }
        internal protected FloatWindow(DockPanel dockPanel, Rectangle bounds)
        {
            Ctor(dockPanel, bounds);
        }
        void Ctor(DockPanel dockPanel, Rectangle bounds)
        {
            ParamGuard.NotNull(dockPanel, "dockPanel");
            NestedGrids = new NestedGridCollection(this);
            NestedGrids.DockGridRemoved += NestedGrids_DockGridRemoved;
            AllowEndUserDocking = true;
            DoubleClickTitleBarToDock = true;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            RightToLeft = dockPanel.RightToLeft;
            RightToLeftLayout = dockPanel.RightToLeftLayout;

            SuspendLayout();
            if (bounds.IsEmpty)
            {
                StartPosition = FormStartPosition.WindowsDefaultLocation;
                Size = dockPanel.DefaultFloatWindowSize;
            }
            else
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = bounds;
            }

            DockPanel = dockPanel;
            Owner = DockPanel.FindForm();
            DockPanel.AddFloatWindow(this);
            ResumeLayout();
        }

        void NestedGrids_DockGridRemoved(object sender, EventArgs e)
        {
            if (NestedGrids.Count == 0)//检查是否包含Grid，如果没有Grid则关闭该FloatWindow
                this.Close();
        }
        public bool AllowEndUserDocking { get; set; }
        public bool DoubleClickTitleBarToDock { get; set; }
        public DockPanel DockPanel { get; private set; }
        public NestedGridCollection NestedGrids { get; private set; }

        internal void UpdateTitle() //如果有多个可见Grid，则不显示标题和图标，否则显示当前激活Content的标题和图标
        {
            var theOnlyGrid = NestedGrids.SingleOrDefault();
            if (theOnlyGrid == null || theOnlyGrid.ActiveContent == null)
            {
                Text = " ";//使用" "空格，防止标题栏消失
                Icon = null;
            }
            else
            {
                Text = theOnlyGrid.ActiveContent.Handler.TabText;
                Icon = theOnlyGrid.ActiveContent.Handler.Icon;
            }
        }
        int preDragExStyle;
        public Rectangle BeginDrag(Point ptMouse)
        {
            preDragExStyle = User32.GetWindowLong(this.Handle, GetWindowLongIndex.GWL_EXSTYLE);
            User32.SetWindowLong(this.Handle, (int)GetWindowLongIndex.GWL_EXSTYLE,
                                        preDragExStyle | (int)(WindowExStyles.WS_EX_TRANSPARENT | Win32.WindowExStyles.WS_EX_LAYERED));
            return Bounds;
        }
        public void EndDrag()
        {
            User32.SetWindowLong(this.Handle, (int)GetWindowLongIndex.GWL_EXSTYLE, preDragExStyle);
            Refresh();
        }
        public bool IsDockValid(DockStyle dockStyle)
        {
            return NestedGrids.SelectMany(g => g.Contents).All(c => dockStyle.IsValid(c.Handler.DockAreas));
        }
        public void FloatAt(Rectangle floatWindowBounds)
        {
            Bounds = floatWindowBounds;
        }
        public Control DragControl
        {
            get { return this; }
        }
        public DockStyle DockStyle { get; private set; }
        public bool IsFloat
        {
            get { return true; }
        }
        internal void TestDrop(IDockDragSource dragSource, DockOutline dockOutline)
        {
            var grid = NestedGrids.SingleOrDefault();
            if (grid == null)
                return;
            if (!dragSource.CanDockTo(grid))
                return;
            var ptMouse = Cursor.Position;
            int lParam = (ptMouse.X << 16) + ptMouse.Y;
            if (User32.SendMessage(Handle, WinMsg.WM_NCHITTEST, 0, lParam) == (IntPtr)HitTest.Caption)
            {
                dockOutline.Show(grid, -1);
            }
        }

        public bool CanDockTo(DockGrid grid)
        {
            if (!IsDockValid(grid.DockStyle))
                return false;

            if (grid.Container == this)
                return false;

            return true;
        }
        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            if (panel != DockPanel)
                throw new ArgumentException("容器DockPanel无效", "panel");

            var nestedGridsTo = DockPanel.DockWindows[dockStyle].NestedGrids;
            var prevGrid = nestedGridsTo.FirstOrDefault(g => g != NestedGrids[0]);
        }
        public void DockTo(DockGrid grid, DockStyle dockStyle, int contentIndex)
        {
            foreach (var g in NestedGrids)
                g.DockTo(grid, dockStyle, contentIndex);
        }
        protected override void OnActivated(EventArgs e)
        {
            DockPanel.AddFloatWindow(this);
            DockPanel.RemoveFloatWindow(this);
            base.OnActivated(e);
            // Propagate the Activated event to the visible panes content objects
            foreach (var content in NestedGrids.SelectMany(g => g.Contents))
                content.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            // Propagate the Deactivate event to the visible panes content objects
            foreach (var content in NestedGrids.SelectMany(g => g.Contents))
                content.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            NestedGrids.Refresh();
            RefreshChanges();
            Visible = NestedGrids.Any();
            UpdateTitle();
            base.OnLayout(levent);
        }
        //保证浮动窗体拖动时始终显示在可见屏幕范围之内
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            var rectWorkArea = SystemInformation.VirtualScreen;

            if (x + width > rectWorkArea.Right)
                x -= (x + width) - rectWorkArea.Right;
            x = Math.Min(x, rectWorkArea.Left);
            if (y + height > rectWorkArea.Bottom)
                y -= (y + height) - rectWorkArea.Bottom;
            y = Math.Min(y, rectWorkArea.Top);

            base.SetBoundsCore(x, y, width, height, specified);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            var contents = NestedGrids.SelectMany(g => g.Contents)
                .Where(c => c.Handler.DockStyle == DockStyle.None && c.Handler.CloseButtonEnabled);
            foreach (var content in contents)
                content.Handler.Close();
        }

        HitTest hitResult;
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)//由于当鼠标位于窗体标题栏内，窗体鼠标事件不会触发，因此必须重载此函数以获取窗体标题栏内的鼠标事件
        {
            switch (m.Msg)
            {
                case (int)WinMsg.WM_NCHITTEST:
                    base.WndProc(ref m);
                    hitResult = (HitTest)m.Result;
                    return;
                case (int)WinMsg.WM_NCLBUTTONDOWN:
                    if (IsDisposed)
                        return;
                    if (hitResult == HitTest.Caption && DockPanel.AllowEndUserDocking && this.AllowEndUserDocking)    // HITTEST_CAPTION
                    {
                        Activate();
                        DockPanel.BeginDrag(this);
                    }
                    else
                        base.WndProc(ref m);
                    return;
                case (int)WinMsg.WM_NCRBUTTONDOWN:
                    if (hitResult == HitTest.Caption)    // HITTEST_CAPTION
                    {
                        var theOnlyGrid = NestedGrids.SingleOrDefault();
                        if (theOnlyGrid != null && theOnlyGrid.ActiveContent != null)
                            theOnlyGrid.ShowTabPageContextMenu(this, PointToClient(Cursor.Position));
                    }
                    else
                        base.WndProc(ref m);
                    return;
                case (int)WinMsg.WM_NCLBUTTONDBLCLK:
                    if (hitResult == HitTest.Caption && DoubleClickTitleBarToDock)
                    {
                        DockPanel.SuspendLayout();
                        foreach (var grid in NestedGrids.Where(g => g.DockStyle == DockStyle.None))
                        {
                            grid.RestoreToPanel();
                        }
                        DockPanel.ResumeLayout(true);
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        internal void RefreshChanges()
        {
            if (IsDisposed)
                return;
            if (!NestedGrids.Any())
            {
                ControlBox = true;
                return;
            }
            var b = NestedGrids.SelectMany(g => g.Contents)
                .Any(c => c.Handler.DockStyle == DockStyle.None && c.Handler.CloseButtonEnabled && c.Handler.CloseButtonVisible);
            if (b)
                ControlBox = true;
            else if (ControlBox)
                ControlBox = false;
        }

    }
}