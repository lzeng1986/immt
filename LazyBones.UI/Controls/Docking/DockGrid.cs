using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using LazyBones.Utils;
using LazyBones.Linq;
using System.ComponentModel;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 用于承载DockContent的格子
    /// </summary>
    [ToolboxItem(false)]
    public class DockGrid : Control, IDockDragSource, ISplitterDragSource
    {
        private Splitter splitter;
        internal DockGrid(IDockContent content, DockGridStyle dockGridStyle)
        {
            ParamGuard.NotNull(content, "content");
            if (content.Handler.DockPanel == null)
                throw new ArgumentNullException("content不属于任何DockPanel");
            SetStyle(ControlStyles.Selectable, false);
            IsFloat = (dockGridStyle.VisibleStyle == DockStyle.None);
            DockPanel = content.Handler.DockPanel;
            DockPanel.AddGrid(this);
            AllowDockDragAndDrop = true;

            splitter = new Splitter(DockPanel);
            DockGridCaption = DockPanel.Extender.NewDockGridCaption(this);
            TabStrip = DockPanel.Extender.NewDockGridStrip(this);
            Controls.AddRange(new Control[] { DockGridCaption, TabStrip, splitter });

            NestedDockStyle = DockStyle.Left;
            DockPanel.SuspendLayout();
            if (IsFloat)
                Container = DockPanel.Extender.NewFloatWindow(DockPanel, dockGridStyle.FloatBounds);
            else if (dockGridStyle.PreviousGrid != null)
                DockTo(dockGridStyle.PreviousGrid.Container, dockGridStyle.PreviousGrid, dockStyle, dockGridStyle.Proportion);

            DockStyle = dockStyle;

            ResumeLayout();
            DockPanel.ResumeLayout(true);
        }

        AutoHideTabCollection autoHideTabs;
        public AutoHideTabCollection AutoHideTabs
        {
            get { return autoHideTabs ?? (autoHideTabs = new AutoHideTabCollection(this)); }
        }

        List<IDockContent> contents = new List<IDockContent>();
        public IList<IDockContent> Contents
        {
            get { return contents; }
        }
        public IEnumerable<IDockContent> DisplayingContents
        {
            get { return contents.Where(o => o.Handler.DockStyle == DockStyle); }
        }
        public DockPanel DockPanel { get; private set; }
        public DockGridCaptionBase DockGridCaption { get; private set; }
        public bool AllowDockDragAndDrop { get; set; }
        public bool IsFloat { get; private set; }
        public bool IsAutoHide { get; set; }
        AutoHideGrid autoHideGrid;
        internal AutoHideGrid AutoHideGrid
        {
            get { return autoHideGrid ?? (autoHideGrid = DockPanel.AutoHideStrip.CreateGrid(this)); }
        }
        public AppearanceStyle Appearance { get; set; }

        internal Rectangle SplitterBounds
        {
            get { return splitter.Bounds; }
            set { splitter.Bounds = value; }
        }

        DockStyle nestedDockStyle = DockStyle.Left;
        internal DockStyle NestedDockStyle
        {
            get { return nestedDockStyle; }
            set
            {
                if (value == DockStyle.None || value == DockStyle.Fill)
                    nestedDockStyle = DockStyle.Right;
                else
                    nestedDockStyle = value;
                switch (value)
                {
                    case DockStyle.Left:
                        splitter.Width = Const.SplitterSize;
                        splitter.Dock = DockStyle.Right;
                        break;
                    case DockStyle.Right:
                        splitter.Width = Const.SplitterSize;
                        splitter.Dock = DockStyle.Left;
                        break;
                    case DockStyle.Top:
                        splitter.Height = Const.SplitterSize;
                        splitter.Dock = DockStyle.Bottom;
                        break;
                    case DockStyle.Bottom:
                        splitter.Height = Const.SplitterSize;
                        splitter.Dock = DockStyle.Top;
                        break;
                }
            }
        }
        internal DockGrid NestedPrevGrid { get; set; }

        double nestedProportion = 0.5;
        internal double NestedProportion
        {
            get { return nestedProportion; }
            set
            {
                if (nestedProportion == value)
                    return;
                if (value < 0 || 1 < value)
                    throw new ArgumentOutOfRangeException("NestedProportion值只能在0和1之间");
                nestedProportion = value;
                if (Container != null)
                    (Container as Control).PerformLayout();
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
                if (!IsDockValid(value))
                    throw new ArgumentException("DockStyle值当前无效");
                IsFloat = (value == DockStyle.None);
                if (IsFloat)
                    Container = DockPanel.Extender.NewFloatWindow(DockPanel,Rectangle.Empty);
                else
                    Container = DockPanel.DockWindows[dockStyle];
                dockStyle = value;
            }
        }
        public void CloseActiveContent()
        {
            CloseContent(ActiveContent);
        }
        internal void CloseContent(IDockContent content)
        {
            if (content == null)
                return;

            if (!content.Handler.CloseButtonEnabled)
                return;
            DockPanel.SuspendLayout();
            try
            {
                content.Handler.Close();
            }
            finally
            {
                DockPanel.ResumeLayout(true);
            }
        }
        IDockContent FocusedContent
        {
            get { return Contents.FirstOrDefault(c => c.Handler.Form.ContainsFocus); }
        }
        HitTestResult GetHitTest(Point screenPoint)
        {
            var ptMouseClient = PointToClient(screenPoint);

            if (CaptionBounds.Contains(ptMouseClient))
                return new HitTestResult(HitTestArea.Caption, -1);

            if (ContentBounds.Contains(ptMouseClient))
                return new HitTestResult(HitTestArea.Content, -1);

            if (TabStripBounds.Contains(ptMouseClient))
                return new HitTestResult(HitTestArea.TabStrip, TabStrip.HitTest(screenPoint));

            return new HitTestResult(HitTestArea.None, -1);
        }
        ContextMenu TabPageContextMenu
        {
            get { return ActiveContent == null ? null : ActiveContent.Handler.TabPageContextMenu; }
        }
        ContextMenuStrip TabPageContextMenuStrip
        {
            get { return ActiveContent == null ? null : ActiveContent.Handler.TabPageContextMenuStrip; }
        }
        internal bool HasTabPageContextMenu
        {
            get { return TabPageContextMenu != null && TabPageContextMenuStrip != null; }
        }
        internal void ShowTabPageContextMenu(Control control, Point position)
        {
            if (TabPageContextMenu != null)
                TabPageContextMenu.Show(control, position);
            else if (TabPageContextMenuStrip != null)
                TabPageContextMenuStrip.Show(this, position);
        }
        internal void TestDrop(IDockDragSource dragSource, DockOutline dockOutline)
        {
            if (!dragSource.CanDockTo(this))
                return;
            HitTestResult hitTestResult = GetHitTest(Cursor.Position);
            if (hitTestResult.HitArea == HitTestArea.Caption)
                dockOutline.Show(this, -1);
            else if (hitTestResult.HitArea == HitTestArea.TabStrip && hitTestResult.Index != -1)
                dockOutline.Show(this, hitTestResult.Index);
        }
        bool HasCaption//Document无标题栏，DockWindow和浮动窗体才有标题栏
        {
            get
            {
                if (DockStyle == DockStyle.Fill)
                    return false;
                if (Container.IsFloat && Container.NestedGrids.Count <= 1)
                    return false;
                return true;
            }
        }
        Rectangle CaptionBounds//获取显示标题栏边界，Document风格无标题栏
        {
            get
            {
                if (!HasCaption)
                    return Rectangle.Empty;
                if (Appearance == AppearanceStyle.Document)
                    return Rectangle.Empty;
                var rect = DisplayRectangle;
                rect.Height = DockGridCaption.CaptionHeight;
                return rect;
            }
        }
        public override Rectangle DisplayRectangle
        {
            get { return ClientRectangle; }
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            Visible = DisplayingContents.Any();
            if (Visible)
            {
                DockGridCaption.Bounds = CaptionBounds;
                TabStrip.Bounds = TabStripBounds;

                SetContentBounds();

                foreach (var content in Contents.Intersect(DisplayingContents))
                {
                    if (content.Handler.ClipForm && content.Handler.Form.Visible)
                        content.Handler.ClipForm = false;
                }
            }
            base.OnLayout(e);
        }
        internal void SetContentBounds()
        {
            var rectContent = ContentBounds;
            var rectInactive = rectContent;
            rectInactive.Offset(-rectInactive.Width, 0);

            foreach (var content in Contents.Where(c => c.Handler.DockGrid == this && c != ActiveContent))
                content.Handler.Form.Bounds = rectInactive;

            ActiveContent.Handler.Form.Bounds = rectContent;
        }
        internal Rectangle ContentBounds//获取显示DockContent的边界，及Grid边界减去Caption和TabStrip部分
        {
            get
            {
                var rect = DisplayRectangle;
                var captionHeigth = HasCaption ? DockGridCaption.CaptionHeight : 0;
                var tabStripHeigth = TabStripBounds.Height;

                rect.Y += captionHeigth;
                rect.Height -= captionHeigth;

                if (DockStyle == DockStyle.Fill) //除了DockStyle.Fill状态的TabStrip在Content上方，其余的TabStrip均在Content下方
                    rect.Y += tabStripHeigth;

                rect.Height -= tabStripHeigth;
                return rect;
            }
        }
        internal Rectangle TabStripBounds
        {
            get
            {
                if (Appearance == AppearanceStyle.ToolWindow && DisplayingContents.Count() <= 1)
                    return Rectangle.Empty;
                if (IsAutoHide)
                    return Rectangle.Empty;
                var rect = DisplayRectangle;
                rect.Height = TabStrip.StripHeight;
                rect.Y = CaptionBounds.Bottom;
                return rect;
            }
        }
        public void RestoreToPanel()
        {
            if (!IsFloat)
                return;
            DockPanel.SuspendLayout();

            foreach (var content in Contents)
                content.Handler.IsFloat = false;

            DockPanel.ResumeLayout(true);
        }
        INestedGridsContainer container = null;
        internal INestedGridsContainer Container//获取或设置包含此Grid的容器
        {
            get { return container; }
            set
            {
                if (container == value)
                    return;
                DockTo(value);
            }
        }
        public Rectangle ParentBounds { get; set; }
        /// <summary>
        /// 获取Grid所在的DockGridStrip
        /// </summary>
        internal DockGridStripBase TabStrip { get; private set; }

        public string CaptionText
        {
            get { return activeContent == null ? string.Empty : activeContent.Handler.TabText; }
        }

        internal void RemoveContent(IDockContent content)
        {
            contents.Remove(content);
            if (contents.Count == 0)
                this.Dispose();
            if (!IsDisposed)
                RefreshGrid();
        }
        internal void AddContent(IDockContent content)
        {
            ParamGuard.NotNull(content, "content");
            if (contents.Contains(content))
                return;
            if (!content.Handler.IsDockValid(DockStyle))
                throw new InvalidOperationException("此DockGrid的Dock属性与此Content不匹配");
            contents.Add(content);
            if (!IsDisposed)
                RefreshGrid();
        }
        internal void AddContents(IEnumerable<IDockContent> contents)
        {
            foreach (var content in contents)
                AddContent(content);
        }
        private IDockContent activeContent = null;
        public virtual IDockContent ActiveContent
        {
            get { return activeContent; }
            set
            {
                if (activeContent == value)
                    return;

                if (value != null)
                {
                    if (value.Handler.DockGrid != this)
                        throw new InvalidOperationException("该Content不属于此DockGrid");
                }
                else
                {
                    if (contents.Count > 0)
                        throw new InvalidOperationException("DisplayingContents不为空，不能设置ActiveContent为null值");
                }

                var oldValue = ActiveContent;

                if (DockPanel.ActiveAutoHideContent == oldValue)
                    DockPanel.ActiveAutoHideContent = null;

                activeContent = value;

                if (DockStyle == DockStyle.Fill)
                {
                    if (activeContent != null)
                        activeContent.Handler.Form.BringToFront();
                }
                else
                {
                    if (activeContent != null)
                        activeContent.Handler.Visible = true;
                    if (oldValue != null)
                        oldValue.Handler.Visible = false;
                    if (IsActivated && activeContent != null)
                        activeContent.Handler.Activate();
                }

                if (Container != null && Container.IsFloat)
                    (Container as FloatWindow).UpdateTitle();

                if (DockStyle == DockStyle.Fill)
                    RefreshChanges(false);  // delayed layout to reduce screen flicker
                else
                    RefreshChanges(true);

                if (activeContent != null)
                    TabStrip.EnsureTabVisible(activeContent);
            }
        }
        public void SetContentIndex(IDockContent content, int index)
        {
            int oldIndex = Contents.IndexOf(content);
            if (oldIndex == -1)
                throw new ArgumentException("不存在指定content", "content");

            if (index < -1 || Contents.Count <= index)
                throw new ArgumentOutOfRangeException("index");
            if (oldIndex == index)
                return;

            Contents.Remove(content);
            if (index == -1)
                Contents.Add(content);
            else
                Contents.Insert(index - 1, content);

            RefreshChanges();
        }
        internal void RefreshGrid()
        {
            RefreshChanges();
            ValidateActiveContent();
        }
        internal void RefreshChanges()
        {
            RefreshChanges(true);
        }
        void RefreshChanges(bool performLayout)//刷新标题栏和Tab标签栏及自动隐藏Strip
        {
            if (IsDisposed)
                return;

            DockGridCaption.RefreshChanges();
            TabStrip.RefreshChanges();
            if (IsFloat && Container != null)
                (Container as FloatWindow).RefreshChanges();
            if (IsAutoHide && DockPanel != null)
            {
                DockPanel.RefreshAutoHideStrip();
                DockPanel.PerformLayout();
            }

            if (performLayout)
                PerformLayout();
        }
        void ValidateActiveContent()
        {
            if (ActiveContent == null)
            {
                ActiveContent = contents.FirstOrDefault();
                return;
            }

            if (contents.Contains(ActiveContent))
                return;

            var ind = contents.IndexOf(activeContent);
            if (ind > 0)
                ActiveContent = contents[ind - 1];
            else if (ind == 0 && contents.Count > 1)
                ActiveContent = contents[ind + 1];
            else
                ActiveContent = null;
        }
        public Rectangle BeginDrag(Point ptMouse)
        {
            var location = PointToScreen(new Point(0, 0));
            Size size;

            var grid = ActiveContent.Handler.DockGrid;
            if (DockStyle == DockStyle.None || grid == null || grid.Container.NestedGrids.Count != 1)
                size = DockPanel.DefaultFloatWindowSize;
            else
                size = grid.Container.Size;

            if (ptMouse.X > location.X + size.Width)
                location.X += ptMouse.X - (location.X + size.Width) + Const.SplitterSize;

            return new Rectangle(location, size);
        }
        public void EndDrag()
        {
        }
        public bool IsDockValid(DockStyle dockStyle)
        {
            return contents.All(c => c.Handler.IsDockValid(dockStyle));
        }
        public void FloatAt(Rectangle floatWindowBounds)
        {
            DockStyle = DockStyle.None;
            if (Container.NestedGrids.Count > 1)
                Container = DockPanel.Extender.NewFloatWindow(DockPanel, floatWindowBounds);
            (Container as FloatWindow).Bounds = floatWindowBounds;
        }
        public Control DragControl
        {
            get { return this; }
        }
        public bool CanDockTo(DockGrid grid)
        {
            if (grid == this)
                return false;
            if (!IsDockValid(grid.DockStyle))
                return false;
            return true;
        }

        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            DockPanel = panel;
            DockStyle = dockStyle;
        }
        public void DockTo(DockGrid grid, DockStyle dockStyle, int contentIndex)
        {
            if (dockStyle == DockStyle.Fill)
            {
                var activeContent = ActiveContent;
                foreach (var content in Contents.Reverse())
                {
                    content.Handler.DockGrid = grid;
                    if (contentIndex != -1)
                        grid.SetContentIndex(content, contentIndex);
                }
                grid.ActiveContent = activeContent;
                this.container = grid.Container;
            }
            else
            {
                DockTo(grid.Container, grid, dockStyle, 0.5);
            }
        }
        internal void DockTo(INestedGridsContainer container)
        {
            ParamGuard.NotNull(container, "container");
            DockTo(container, null, container.DockStyle, 0.5);
        }
        internal void DockTo(INestedGridsContainer container, DockGrid prevGrid, DockStyle nestedDockStyle, double proportion)
        {
            ParamGuard.NotNull(container, "container");
            if (container.DockStyle != this.DockStyle)
                throw new InvalidOperationException();
            if (nestedDockStyle == DockStyle.Fill || nestedDockStyle == DockStyle.None)
                return;
            if (prevGrid == this)
                throw new InvalidOperationException("prevGrid不能为指向当前Grid");

            if (container != null)
                container.NestedGrids.Remove(this);

            NestedDockStyle = nestedDockStyle;
            NestedProportion = proportion;
            container.NestedGrids.Insert(this, prevGrid);
            var oldContainer = Container as Control;
            if (oldContainer != null && !oldContainer.IsDisposed)
                oldContainer.PerformLayout();
            this.container = container;
            (this.container as Control).PerformLayout();
        }

        int refreshStateChangeCount = 0;
        bool IsRefreshStateChangeSuspended
        {
            get { return refreshStateChangeCount > 0; }
        }
        void SuspendRefreshDockChange()
        {
            refreshStateChangeCount++;
            DockPanel.SuspendLayout();
        }

        void ResumeRefreshDockChange()
        {
            if (refreshStateChangeCount == 0)
                return;
            refreshStateChangeCount--;
            DockPanel.ResumeLayout(true);
        }
        void ResumeRefreshDockChange(INestedGridsContainer oldContainer, DockStyle oldDockStyle)
        {
            ResumeRefreshDockChange();
            RefreshContainer(oldContainer as Control);
        }
        void RefreshContainer(Control oldContainer)
        {
            if (IsRefreshStateChangeSuspended)
                return;

            SuspendRefreshDockChange();

            DockPanel.SuspendLayout();

            var contentFocused = FocusedContent;
            if (contentFocused != null)
                DockPanel.SaveFocus();
            SetParent();

            if (ActiveContent != null)
                ActiveContent.Handler.DockStyle = DockStyle;

            foreach (var content in Contents.Where(c => c.Handler.DockGrid == this))
                content.Handler.DockStyle = DockStyle;

            if (oldContainer != null && !oldContainer.IsDisposed)
                oldContainer.PerformLayout();
            if (IsAutoHide)
                DockPanel.RefreshActiveAutoHideContent();
            (Container as Control).PerformLayout();

            if (IsAutoHide)
            {
                DockPanel.RefreshActiveAutoHideContent();
                DockPanel.RefreshAutoHideStrip();
                DockPanel.PerformLayout();
            }

            ResumeRefreshDockChange();

            if (contentFocused != null)
                contentFocused.Handler.Activate();

            DockPanel.ResumeLayout(true);
        }

        void SetParent()
        {
            if (!Visible)
            {
                Parent = null;
                splitter.Parent = null;
            }
            else if (DockStyle == DockStyle.None)
            {
                Parent = Container as Control;
                splitter.Parent = Container as Control;
            }
            else if (IsAutoHide)
            {
                Parent = DockPanel.AutoHideControl;
                splitter.Parent = null;
            }
            else
            {
                Parent = DockPanel.DockWindows[DockStyle];
                splitter.Parent = Parent;
            }
        }
        public new Control Parent
        {
            get { return base.Parent; }
            set
            {
                if (base.Parent == value)
                    return;

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Workaround of .Net Framework bug:
                // Change the parent of a control with focus may result in the first
                // MDI child form get activated. 
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                IDockContent contentFocused = FocusedContent;
                if (contentFocused != null)
                    DockPanel.SaveFocus();

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                base.Parent = value;

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Workaround of .Net Framework bug:
                // Change the parent of a control with focus may result in the first
                // MDI child form get activated. 
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (contentFocused != null)
                    contentFocused.Handler.Activate();
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
        }

        DockGrid GetFloatGridFromContents()
        {
            return DisplayingContents.Where(c => c.Handler.IsDockValid(DockStyle.None)).Select(c => c.Handler.DockGrid).SingleOrDefault();
        }
        IDockContent GetFirstContent(DockStyle dockStyle)
        {
            return DisplayingContents.FirstOrDefault(c => c.Handler.IsDockValid(dockStyle));
        }

        bool isActivated = false;
        public bool IsActivated
        {
            get { return isActivated; }
            internal set
            {
                if (isActivated == value)
                    return;
                isActivated = value;
                if (DockStyle != DockStyle.Fill)
                    RefreshChanges(false);
                OnIsActivatedChanged(EventArgs.Empty);
            }
        }
        bool isActiveDocumentGrid = false;
        public bool IsActiveDocumentGrid
        {
            get { return isActiveDocumentGrid; }
            internal set
            {
                if (isActiveDocumentGrid == value)
                    return;
                isActiveDocumentGrid = value;
                if (DockStyle == DockStyle.Fill)
                    RefreshChanges();
                OnIsActiveDocumentGridChanged(EventArgs.Empty);
            }
        }
        public void BeginDrag(Rectangle splitterScreenBounds)
        {
        }
        public bool IsVertical
        {
            get { return NestedDockStyle == DockStyle.Left || NestedDockStyle == DockStyle.Right; }
        }
        public Rectangle DragLimitBounds //ParentBounds减去GridSize的范围
        {
            get
            {
                var limitRect = ParentBounds;
                if (this.IsVertical)
                    limitRect.Inflate(-Const.MinGridSize, 0);
                else
                    limitRect.Inflate(0, -Const.MinGridSize);
                return Parent.RectangleToScreen(limitRect);
            }
        }

        public void MoveSplitter(int offset)
        {
            var proportion = NestedProportion;
            if (ParentBounds.Width <= 0 || ParentBounds.Height <= 0)
                return;
            if (NestedDockStyle == DockStyle.Left)
                proportion += offset / (double)ParentBounds.Width;
            else if (NestedDockStyle == DockStyle.Right)
                proportion -= offset / (double)ParentBounds.Width;
            else if (NestedDockStyle == DockStyle.Top)
                proportion += offset / (double)ParentBounds.Height;
            else
                proportion -= offset / (double)ParentBounds.Height;

            NestedProportion = proportion;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Container != null)
                    Container.NestedGrids.Remove(this);
                if (DockPanel != null)
                {
                    DockPanel.RemoveGrid(this);
                    DockPanel = null;
                }
                splitter.Dispose();
                splitter = null;
                if (AutoHideGrid != null)
                    AutoHideGrid.Dispose();
            }
            base.Dispose(disposing);
        }
        public event EventHandler IsActiveDocumentGridChanged;
        protected virtual void OnIsActiveDocumentGridChanged(EventArgs e)
        {
            if (IsActiveDocumentGridChanged != null)
                IsActiveDocumentGridChanged(this, e);
        }
        public event EventHandler IsActivatedChanged;
        protected virtual void OnIsActivatedChanged(EventArgs e)
        {
            if (IsActivatedChanged != null)
                IsActivatedChanged(this, e);
        }
    }

    internal struct HitTestResult
    {
        public readonly HitTestArea HitArea;
        public readonly int Index;
        public HitTestResult(HitTestArea hitTestArea, int index)
        {
            HitArea = hitTestArea;
            Index = index;
        }
    }
}
