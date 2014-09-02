using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LazyBones.Extensions;
using LazyBones.Utils;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    public class DockContentHandler : IDisposable, IDockDragSource
    {
        public static DockContentHandler CreateFrom<T>(T form)
            where T : Form, IDockContent
        {
            return new DockContentHandler(form, form);
        }
        internal DockContentHandler(Form form, IDockContent dockContent)
        {
            Form = form;
            Icon = form.Icon;
            Content = dockContent;
            form.Disposed += form_Disposed;
            form.TextChanged += form_TextChanged;
            AllowEndUserDocking = true;
            ActiveWindowHandle = IntPtr.Zero;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DockPanel = null;
                if (GridStripTab != null)
                {
                    GridStripTab.Dispose();
                    GridStripTab = null;
                }
                if (AutoHideTab != null)
                {
                    AutoHideTab.Dispose();
                    AutoHideTab = null;
                }
                Form.Disposed -= form_Disposed;
                Form.TextChanged -= form_TextChanged;
                Form = null;
                Content = null;
                Icon = null;
            }
        }
        void form_TextChanged(object sender, EventArgs e)
        {
            if (IsAutoHide)
                DockPanel.RefreshAutoHideStrip();
            else if (DockGrid != null)
            {
                if (DockGrid.Container != null && DockGrid.Container.IsFloat)
                    (DockGrid.Container as FloatWindow).UpdateTitle();
                DockGrid.RefreshChanges();
            }
        }
        void form_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        public Form Form { get; private set; }
        public IDockContent Content { get; private set; }
        public Icon Icon { get; private set; }
        public bool IsAutoHide { get; set; }
        public Control DragControl { get { return Form; } }
        public IDockContent PreviousActiveContent { get; set; }
        public IDockContent NextActiveContent { get; set; }
        public bool AllowEndUserDocking { get; set; }
        internal IntPtr ActiveWindowHandle { get; set; }
        public ContextMenu TabPageContextMenu { get; set; }
        public ContextMenuStrip TabPageContextMenuStrip { get; set; }
        public string ToolTipText { get; set; }
        public bool IsActivated { get; internal set; }

        double autoHidePortion = 0.25;
        public double AutoHidePortion
        {
            get { return autoHidePortion; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                if (autoHidePortion == value)
                    return;
                autoHidePortion = value;
                if (DockPanel != null && DockPanel.ActiveAutoHideContent == this.Content)
                    DockPanel.PerformLayout();
            }
        }

        bool closeButtonEnabled = true;
        public bool CloseButtonEnabled
        {
            get { return closeButtonEnabled; }
            set
            {
                if (closeButtonEnabled == value)
                    return;
                closeButtonEnabled = value;
                if (IsActiveContentHandler)
                    DockGrid.RefreshChanges();
            }
        }

        bool closeButtonVisible = true;
        public bool CloseButtonVisible
        {
            get { return closeButtonVisible; }
            set
            {
                if (closeButtonVisible == value)
                    return;

                closeButtonVisible = value;
                if (IsActiveContentHandler)
                    DockGrid.RefreshChanges();
            }
        }

        bool IsActiveContentHandler
        {
            get { return DockGrid.ActiveContent == this.Content; }
        }

        DockGrid dockGrid = null;
        public DockGrid DockGrid    //设置或获取该Content停靠的DockGrid
        {
            get { return dockGrid; }
            set //先从原始DockGrid中删除此DockContent，然后再在新的DockGrid中添加此DockContent
            {
                if (dockGrid == value)
                    return;

                DockPanel.SuspendLayout();
                SuspendSetDockStyle();
                if (dockGrid != null)
                {
                    dockGrid.RemoveContent(this.Content);
                    ClipForm = true;
                    Parent = null;
                }

                dockGrid = value;

                if (dockGrid != null)
                {
                    dockGrid.AddContent(Content);
                    DockStyle = dockGrid.DockStyle;
                    Parent = dockGrid;
                }
                ResumeSetDockStyle();
                DockStyle = value == null ? DockStyle.None : value.DockStyle;
                DockPanel.ResumeLayout(true);
            }
        }
        internal Control Parent
        {
            get { return Form.Parent; }
            set
            {
                if (Form.Parent == value)
                    return;

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Workaround of .Net Framework bug:
                // Change the parent of a control with focus may result in the first
                // MDI child form get activated. 
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                bool bRestoreFocus = false;
                if (Form.ContainsFocus)
                {
                    // Suggested as a fix for a memory leak by bugreports
                    if (value == null && !IsFloat)
                    {
                        DockPanel.ContentFocusManager.GiveUpFocus(this.Content);
                    }
                    else
                    {
                        DockPanel.SaveFocus();
                        bRestoreFocus = true;
                    }
                }

                if (Form.TopLevel)
                    Form.TopLevel = false;
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                Form.Parent = value;

                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Workaround of .Net Framework bug:
                // Change the parent of a control with focus may result in the first
                // MDI child form get activated. 
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (bRestoreFocus)
                    Activate();

                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
        }
        public bool Visible
        {
            get { return Form.Visible; }
            set
            {
                if (Form.Visible == value)
                    return;
                if (value)
                {
                    DockPanel.ContentFocusManager.AddToList(Content);
                    if (DockGrid != null)
                        Form.Visible = (DockGrid.ActiveContent == Content);
                }
                else
                {
                    if (Form.ContainsFocus)
                        DockPanel.ContentFocusManager.GiveUpFocus(Content);
                    DockPanel.ContentFocusManager.RemoveFromList(Content);
                    Form.Visible = false;
                }
                DockGrid.RefreshGrid();
            }
        }

        string tabText = null;
        public string TabText
        {
            get { return string.IsNullOrEmpty(tabText) ? Form.Text : tabText; }
            set
            {
                if (tabText == value)
                    return;
                tabText = value;
                if (DockGrid != null)
                    DockGrid.RefreshChanges();
            }
        }

        public void Activate()
        {
            if (DockPanel == null)
            {
                Form.Activate();
            }
            else
            {
                Visible = false;
                if (DockGrid == null)
                    DockGrid = DockPanel.Extender.NewDockGrid(Content, new DockGridStyle { VisibleStyle = DockStyle });
                DockGrid.ActiveContent = this.Content;
                if (DockStyle == DockStyle.Fill)
                {
                    Form.Activate();
                    return;
                }
                if (IsAutoHide && dockPanel.ActiveAutoHideContent != this.Content)
                {
                    dockPanel.ActiveAutoHideContent = null;
                    return;
                }

                if (Form.ContainsFocus)
                    return;

                dockPanel.ContentFocusManager.Activate(Content);
            }
        }

        public void GiveUpFocus()
        {
            dockPanel.ContentFocusManager.GiveUpFocus(Content);
        }
        public void Show()//当不在dockPanel中时，直接显示窗体，否则在dockPanel中显示该窗体
        {
            if (DockPanel == null)
                Form.Show();
            else
                Activate();
        }

        public void Show(DockPanel dockPanel)
        {
            ParamGuard.NotNull(dockPanel, "dockPanel");
            DockPanel = dockPanel;
            Activate();
        }
        public void Show(DockPanel dockPanel, DockStyle dockStyle)
        {
            ParamGuard.NotNull(dockPanel, "dockPanel");

            DockPanel.SuspendLayout();

            DockPanel = dockPanel;

            var gridExisting = DockPanel.Grids.FirstOrDefault(g => g.DockStyle == dockStyle && g.IsActivated);
            DockGrid = gridExisting ?? DockPanel.Extender.NewDockGrid(Content, new DockGridStyle { VisibleStyle = dockStyle });

            DockPanel.ResumeLayout(true); //we'll resume the layout before activating to ensure that the position
            Activate();                         //and size of the form are finally processed before the form is shown
        }
        public void Show(DockPanel dockPanel, Rectangle floatWindowBounds)
        {
            ParamGuard.NotNull(dockPanel, "dockPanel");
            DockPanel.SuspendLayout();

            DockPanel = dockPanel;
            if (DockGrid == null)
            {
                DockGrid = DockPanel.Extender.NewDockGrid(Content, new DockGridStyle { FloatBounds = floatWindowBounds });
            }
            if (DockGrid.IsFloat)
            {
                Visible = false;    //减少屏幕闪烁
                (DockGrid.Container as FloatWindow).StartPosition = FormStartPosition.Manual;
                (DockGrid.Container as FloatWindow).Bounds = floatWindowBounds;
            }

            Show(dockPanel, DockStyle.None);
            Activate();

            DockPanel.ResumeLayout(true);
        }
        public void Show(DockGrid grid, IDockContent prevContent)
        {
            ParamGuard.NotNull(grid, "grid");
            ParamGuard.NotNull(prevContent, "prevContent");
            var ind = grid.Contents.IndexOf(prevContent);
            if (ind == -1)
                throw new ArgumentException("prevContent");

            grid.DockPanel.SuspendLayout();

            DockPanel = grid.DockPanel;
            DockGrid = grid;

            grid.SetContentIndex(Content, ind);
            Activate();

            grid.DockPanel.ResumeLayout(true);
        }
        public void Show(DockGrid previousGrid, DockStyle dockStyle, double proportion)
        {
            ParamGuard.NotNull(previousGrid, "previousGrid");

            if (previousGrid.IsAutoHide)
                throw new ArgumentException("DockContentHandler_Show_InvalidPrevPane");

            previousGrid.DockPanel.SuspendLayout();

            DockPanel = previousGrid.DockPanel;
            //DockPanel.DockGridFactory.CreateDockGrid(Content, previousGrid, dockStyle, proportion);
            Show();

            previousGrid.DockPanel.ResumeLayout(true);
        }

        public void Close()
        {
            if (HideOnClose)
            {
                Visible = false;
            }
            else
            {
                if (dockPanel != null)
                    DockPanel.SuspendLayout();
                Form.Close();
                if (dockPanel != null)
                    DockPanel.ResumeLayout(true);
            }
        }
        public bool HideOnClose { get; set; }
        bool isFloat = false;
        public bool IsFloat
        {
            get { return isFloat; }
            set
            {
                if (isFloat == value)
                    return;
                isFloat = value;
                if (isFloat)
                    DockStyle = DockStyle.None;
                else
                    DockStyle = DockGrid == null ? DefaultDockStyle : DockGrid.DockStyle;
            }
        }
        private bool clipForm = false;
        internal bool ClipForm
        {
            get { return clipForm; }
            set
            {
                if (clipForm == value)
                    return;
                clipForm = value;
                Form.Region = clipForm ? new Region(Rectangle.Empty) : null;
            }
        }

        int setDockStateCount = 0;
        void SuspendSetDockStyle()
        {
            setDockStateCount++;
        }
        void ResumeSetDockStyle()
        {
            setDockStateCount--;
            if (setDockStateCount < 0)
                setDockStateCount = 0;
        }
        void ResumeSetDockStyle(bool visible, DockStyle dockStyle, DockGrid oldGrid)
        {
            ResumeSetDockStyle();
            Visible = visible;
            DockStyle = dockStyle;
            DockGrid = oldGrid;
        }
        internal bool IsSuspendSetDockStyle
        {
            get { return setDockStateCount != 0; }
        }

        DockStyle DefaultDockStyle
        {
            get
            {
                if (dockPosition.HasFlag(DockAreas.Document))
                    return DockStyle.Fill;
                if (dockPosition.HasFlag(DockAreas.Right))
                    return DockStyle.Right;
                if (dockPosition.HasFlag(DockAreas.Left))
                    return DockStyle.Left;
                if (dockPosition.HasFlag(DockAreas.Bottom))
                    return DockStyle.Bottom;
                if (dockPosition.HasFlag(DockAreas.Top))
                    return DockStyle.Top;
                return DockStyle.None;
            }
        }

        DockAreas dockPosition = DockAreas.All;
        public DockAreas DockAreas
        {
            get { return dockPosition; }
            set
            {
                if (dockPosition == value)
                    return;
                if (!DockStyle.IsValid(value))
                    throw new InvalidOperationException("当前DockStyle不支持该DockAreas");
                dockPosition = value;
            }
        }

        DockStyle dockStyle;
        public DockStyle DockStyle
        {
            get { return dockStyle; }
            set
            {
                if (dockStyle == value)
                    return;
                if (value.IsValid(DockAreas))
                    throw new ArgumentException("当前DockStyle不支持该DockAreas");
                isFloat = value == DockStyle.None;
                if (!Visible || IsAutoHide)
                    DockPanel.ContentFocusManager.RemoveFromList(Content);
                else
                    DockPanel.ContentFocusManager.AddToList(Content);

                AutoHidePortion = DockPanel.DockWindows[DockStyle].AutoHidePortion;

                DockPanel.SuspendLayout();
                Visible = true;
                DockPanel.ResumeLayout(true);
            }
        }

        internal StripTab GridStripTab { get; set; }
        internal StripTab AutoHideTab { get; set; }
        internal AutoHideTabCollection AutoHideTabs { get; set; }
        DockPanel dockPanel = null;
        public DockPanel DockPanel  //设置或获取该Handler所属的DockPanel
        {
            get { return dockPanel; }
            set
            {
                if (dockPanel == value)
                    return;

                DockGrid = null;

                if (dockPanel != null)
                    dockPanel.RemoveContent(Content);

                if (GridStripTab != null)
                {
                    GridStripTab.Dispose();
                    GridStripTab = null;
                }
                if (AutoHideTab != null)
                {
                    AutoHideTab.Dispose();
                    AutoHideTab = null;
                }

                dockPanel = value;

                if (dockPanel == null)
                    return;

                dockPanel.AddContent(Content);
                //设置窗体属性
                Form.TopLevel = false;
                Form.FormBorderStyle = FormBorderStyle.None;
                Form.ShowInTaskbar = false;
                Form.WindowState = FormWindowState.Normal;

                User32.SetWindowPos(Form.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    SetWindowPosFlags.SWP_NOACTIVATE |
                    SetWindowPosFlags.SWP_NOMOVE |
                    SetWindowPosFlags.SWP_NOSIZE |
                    SetWindowPosFlags.SWP_NOZORDER |
                    SetWindowPosFlags.SWP_NOOWNERZORDER |
                    SetWindowPosFlags.SWP_FRAMECHANGED);
            }
        }

        public Rectangle BeginDrag(Point ptMouse)
        {
            Size size;
            if (DockGrid == null || !DockGrid.IsFloat)
                size = DockPanel.DefaultFloatWindowSize;
            else
                size = DockGrid.Container.Size;

            var location = DockGrid.PointToScreen(DockGrid.Location);

            if (ptMouse.X > location.X + size.Width)
                location.X += ptMouse.X - (location.X + size.Width) + Const.SplitterSize;

            return new Rectangle(location, size);
        }

        public void EndDrag()
        {

        }
        public bool IsDockValid(DockStyle dockStyle)
        {
            return dockStyle.IsValid(dockPosition);
        }

        public void FloatAt(Rectangle floatWindowBounds)
        {
            DockPanel.Extender.NewDockGrid(Content, new DockGridStyle { FloatBounds = floatWindowBounds });
        }

        public void DockTo(DockStyle dockStyle)
        {
            if (dockStyle != DockStyle.None)
                this.DockGrid = DockPanel.Extender.NewDockGrid(Content, new DockGridStyle { VisibleStyle = dockStyle });
        }
        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            throw new NotImplementedException();
        }
        public void DockTo(DockGrid grid, DockStyle dockStyle, int contentIndex)
        {
            if (dockStyle == DockStyle.Fill)
            {
                DockGrid = grid;
                grid.SetContentIndex(Content, contentIndex);
            }
            else
            {
                var gridFrom = dockPanel.Extender.NewDockGrid(Content, new DockGridStyle { VisibleStyle = grid.DockStyle });
                var container = grid.Container;
                if (dockStyle != DockStyle.None)
                    gridFrom.DockTo(container, grid, dockStyle, 0.5);
                gridFrom.DockStyle = grid.DockStyle;
            }
        }

        public bool CanDockTo(DockGrid grid)
        {
            if (IsDockValid(grid.DockStyle))
                return true;

            if (DockGrid == grid && grid.DisplayingContents.Count() == 1)
                return false;

            return true;
        }
    }
}
