using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 实现自动隐藏功能
    /// </summary>
    internal class AutoHideWindow : Panel, ISplitterDragSource
    {
        const int ANIMATE_TIME = 100;

        Timer timerMouseTrack;
        Splitter splitter;

        public AutoHideWindow(DockPanel dockPanel)
        {
            DockPanel = dockPanel;

            timerMouseTrack = new Timer();
            timerMouseTrack.Tick += TimerMouseTrack_Tick;

            Visible = false;
            splitter = new Splitter(dockPanel);
            Controls.Add(splitter);

            if (Dock == DockStyle.Left)
            {
                splitter.Dock = DockStyle.Right;
            }
            else if (Dock == DockStyle.Right)
            {
                splitter.Dock = DockStyle.Left;
            }
            else if (Dock == DockStyle.Top)
            {
                splitter.Dock = DockStyle.Bottom;
            }
            else if (Dock == DockStyle.Bottom)
            {
                splitter.Dock = DockStyle.Top;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timerMouseTrack.Dispose();
            }
            base.Dispose(disposing);
        }

        public DockPanel DockPanel { get; private set; }

        private DockGrid activeGrid = null;
        public DockGrid ActiveGrid
        {
            get { return activeGrid; }
        }

        public event EventHandler ActiveContentChanged;
        protected virtual void OnActiveContentChanged(EventArgs e)
        {
            if (ActiveContentChanged != null)
                ActiveContentChanged(this, e);
        }

        private IDockContent activeContent = null;
        public IDockContent ActiveContent
        {
            get { return activeContent; }
            set
            {
                if (value == activeContent)
                    return;

                if (value != null)
                {
                    if (!value.Handler.IsAutoHide)
                        throw new InvalidOperationException("设置的IDockContent不能被自动隐藏");
                    if (value.Handler.DockPanel != DockPanel)
                        throw new InvalidOperationException("设置的IDockContent不属于当前DockPanel");
                }

                DockPanel.SuspendLayout();

                if (activeContent != null)
                {
                    if (activeContent.Handler.Form.ContainsFocus)
                        DockPanel.ContentFocusManager.GiveUpFocus(activeContent);
                    HideWindow();
                }

                activeContent = value;

                activeGrid = (activeContent == null) ? null : activeContent.Handler.DockGrid;
                if (activeGrid != null)
                    activeGrid.ActiveContent = activeContent;
                if (activeContent != null)
                    ShowWindow();

                DockPanel.ResumeLayout();
                DockPanel.RefreshAutoHideStrip();

                StartMouseTrackTimer();

                OnActiveContentChanged(EventArgs.Empty);
            }
        }
        public override DockStyle Dock
        {
            get { return activeContent == null ? DockStyle.Left : activeContent.Handler.DockStyle; }
            set { }
        }

        bool isAnimate = true;

        bool isDragging = false;
        internal bool IsDragging
        {
            get { return isDragging; }
            set
            {
                if (isDragging == value)
                    return;
                isDragging = value;
                StartMouseTrackTimer();
            }
        }
        void ShowWindow()
        {
            if (!isAnimate && Visible)
            {
                Visible = true;
                return;
            }

            var rectSource = HiddenRect;
            var rectTarget = ShownRect;
            Animation animation = null;
            if (Dock == DockStyle.Left)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Width += 1;
                    return rect.Width < rectTarget.Width;
                };
            }
            else if (Dock == DockStyle.Top)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Height += 1;
                    return rect.Height < rectTarget.Height;
                };
            }
            else if (Dock == DockStyle.Right)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.X -= 1;
                    rect.Width += 1;
                    return rect.Width < rectTarget.Width;
                };
            }
            else if (Dock == DockStyle.Bottom)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Y -= 1;
                    rect.Height += 1;
                    return rect.Height < rectTarget.Height;
                };
            }

            rectTarget.Offset(-rectTarget.Width, -rectTarget.Height);
            Bounds = rectTarget;
            if (Visible == false)
                Visible = true;
            PerformLayout();
            if (animation != null)
                AnimateWindow(rectSource, animation);
        }
        void HideWindow()
        {
            if (!isAnimate && Visible)
            {
                Visible = false;
                return;
            }
            var rectSource = ShownRect;
            Animation animation = null;
            if (Dock == DockStyle.Left)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Width -= 1;
                    return rect.Width > 0;
                };
            }
            else if (Dock == DockStyle.Top)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Height -= 1;
                    return rect.Height > 0;
                };
            }
            else if (Dock == DockStyle.Right)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.X += 1;
                    rect.Width -= 1;
                    return rect.Width > 0;
                };
            }
            else if (Dock == DockStyle.Bottom)
            {
                animation = (ref Rectangle rect) =>
                {
                    rect.Y += 1;
                    rect.Height -= 1;
                    return rect.Height > 0;
                };
            }
            if (animation != null)
                AnimateWindow(rectSource, animation);
        }
        delegate bool Animation(ref Rectangle rect);
        void AnimateWindow(Rectangle rectSource, Animation animation)
        {
            Parent.SuspendLayout();
            SuspendLayout();
            LayoutAnimateWindow(rectSource);
            if (Visible == false)
                Visible = true;
            while (animation(ref rectSource))
            {
                LayoutAnimateWindow(rectSource);
                if (Parent != null)
                    Parent.Update();
                Thread.Sleep(ANIMATE_TIME);
            }
            ResumeLayout();
            Parent.ResumeLayout();
        }

        void LayoutAnimateWindow(Rectangle rect)
        {
            Bounds = rect;
            if (Dock == DockStyle.Left)
                activeGrid.Left = ClientRectangle.Right - 2 - Const.SplitterSize - activeGrid.Width;
            else if (Dock == DockStyle.Top)
                activeGrid.Top = ClientRectangle.Bottom - 2 - Const.SplitterSize - activeGrid.Height;
        }

        Rectangle ShownRect
        {
            get { return DockPanel.AutoHideCtrlBounds; }
        }
        Rectangle HiddenRect
        {
            get
            {
                var rect = DockPanel.AutoHideCtrlBounds;
                if (Dock == DockStyle.Left)
                    rect.Width = 0;
                else if (Dock == DockStyle.Right)
                {
                    rect.X += rect.Width;
                    rect.Width = 0;
                }
                else if (Dock == DockStyle.Top)
                    rect.Height = 0;
                else
                {
                    rect.Y += rect.Height;
                    rect.Height = 0;
                }
                return rect;
            }
        }
        void StartMouseTrackTimer()
        {
            if (activeGrid == null || activeGrid.IsActivated || isDragging)
            {
                timerMouseTrack.Enabled = false;
                return;
            }

            var hoverTime = SystemInformation.MouseHoverTime;
            if (hoverTime <= 0)
                hoverTime = 400;
            timerMouseTrack.Interval = 2 * hoverTime;
            timerMouseTrack.Start();
        }
        public override Rectangle DisplayRectangle
        {
            get
            {
                var rect = ClientRectangle;

                // 除掉边框（宽度为1）和分割栏
                if (Dock == DockStyle.Bottom)
                    rect.X -= 2 + Const.SplitterSize;
                else if (Dock == DockStyle.Right)
                    rect.Y -= 2 + Const.SplitterSize;
                else if (Dock == DockStyle.Top)
                    rect.Height -= 2 + Const.SplitterSize;
                else if (Dock == DockStyle.Left)
                    rect.Width -= 2 + Const.SplitterSize;

                return rect;
            }
        }
        protected override void OnLayout(LayoutEventArgs levent)
        {
            DockPadding.All = 0;
            if (Dock == DockStyle.Left)
            {
                DockPadding.Right = 2;
                splitter.Dock = DockStyle.Right;
            }
            else if (Dock == DockStyle.Right)
            {
                DockPadding.Left = 2;
                splitter.Dock = DockStyle.Left;
            }
            else if (Dock == DockStyle.Top)
            {
                DockPadding.Bottom = 2;
                splitter.Dock = DockStyle.Bottom;
            }
            else if (Dock == DockStyle.Bottom)
            {
                DockPadding.Top = 2;
                splitter.Dock = DockStyle.Top;
            }

            var rectDisplay = DisplayRectangle;
            var rectHidden = new Rectangle(-rectDisplay.Width, rectDisplay.Y, rectDisplay.Width, rectDisplay.Height);
            
            foreach (var grid in Controls.OfType<DockGrid>())
            {
                if (grid == activeGrid)
                    grid.Bounds = rectDisplay;
                else
                    grid.Bounds = rectHidden;
            }
            base.OnLayout(levent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            switch (Dock)
            {
                case DockStyle.Bottom:
                    g.DrawLine(SystemPens.ControlLightLight, 0, 1, ClientRectangle.Right, 1);
                    break;
                case DockStyle.Top:
                    g.DrawLine(SystemPens.ControlDark, 0, ClientRectangle.Height - 2, ClientRectangle.Right, ClientRectangle.Height - 2);
                    g.DrawLine(SystemPens.ControlDarkDark, 0, ClientRectangle.Height - 1, ClientRectangle.Right, ClientRectangle.Height - 1);
                    break;
                case DockStyle.Right:
                    g.DrawLine(SystemPens.ControlLightLight, 1, 0, 1, ClientRectangle.Bottom);
                    break;
                case DockStyle.Left:
                    g.DrawLine(SystemPens.ControlDark, ClientRectangle.Width - 2, 0, ClientRectangle.Width - 2, ClientRectangle.Bottom);
                    g.DrawLine(SystemPens.ControlDarkDark, ClientRectangle.Width - 1, 0, ClientRectangle.Width - 1, ClientRectangle.Bottom);
                    break;
            }

            base.OnPaint(e);
        }

        public void RefreshActiveContent()
        {
            if (activeContent == null || activeContent.Handler.IsAutoHide)
                return;

            isAnimate = false;
            ActiveContent = null;
            isAnimate = true;
        }

        public void RefreshActiveGrid()
        {
            StartMouseTrackTimer();
        }

        private void TimerMouseTrack_Tick(object sender, EventArgs e)
        {
            if (IsDisposed)
                return;

            if (activeGrid == null || activeGrid.IsActivated)
            {
                timerMouseTrack.Enabled = false;
                return;
            }

            var grid = activeGrid;
            var pos = PointToClient(Cursor.Position);
            var posInDockPanel = DockPanel.PointToClient(Cursor.Position);

            Rectangle rectTabStrip = DockPanel.AutoHideStrip.GetTabStripRectangle(grid.DockStyle);

            if (!ClientRectangle.Contains(pos) && !rectTabStrip.Contains(posInDockPanel))
            {
                ActiveContent = null;
                timerMouseTrack.Enabled = false;
            }
        }

        public void BeginDrag(Rectangle splitterScreenBounds)
        {
            IsDragging = true;
        }

        public void EndDrag()
        {
            IsDragging = false;
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
                if (this.IsVertical)
                    limitRect.Inflate(-Const.MinGridSize, 0);
                else
                    limitRect.Inflate(0, -Const.MinGridSize);
                return DockPanel.RectangleToScreen(limitRect);
            }
        }

        public void MoveSplitter(int offset)
        {
            Rectangle rectDockArea = DockPanel.DockBounds;
            IDockContent content = ActiveContent;
            if (Dock == DockStyle.Left && rectDockArea.Width > 0)
            {
                if (content.Handler.AutoHidePortion < 1)
                    content.Handler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Width;
                else
                    content.Handler.AutoHidePortion = Width + offset;
            }
            else if (Dock == DockStyle.Right && rectDockArea.Width > 0)
            {
                if (content.Handler.AutoHidePortion < 1)
                    content.Handler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Width;
                else
                    content.Handler.AutoHidePortion = Width - offset;
            }
            else if (Dock == DockStyle.Bottom && rectDockArea.Height > 0)
            {
                if (content.Handler.AutoHidePortion < 1)
                    content.Handler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Height;
                else
                    content.Handler.AutoHidePortion = Height - offset;
            }
            else if (Dock == DockStyle.Top && rectDockArea.Height > 0)
            {
                if (content.Handler.AutoHidePortion < 1)
                    content.Handler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Height;
                else
                    content.Handler.AutoHidePortion = Height + offset;
            }
        }

        public Control DragControl
        {
            get { return this; }
        }
    }
}
