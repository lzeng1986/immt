using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LazyBones.Utils;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    public class DockPanel : Panel
    {
        FocusManager m_focusManager;

        AutoHideWindow autoHideCtrl;
        DockWindowCollection dockWindows;
        internal readonly Control dummyControl;
        readonly SplitterDragHandler splitterDragHandler;
        FocusManager focusManager;
        public DockPanel()
        {
            splitterDragHandler = new SplitterDragHandler(this);
            focusManager = new FocusManager(this);
            dummyControl = new DummyControl();
            dockWindows = new DockWindowCollection(this);
            autoHideCtrl = new AutoHideWindow(this);
            DockDragHandler = new DockDragHandler(this);
            dummyControl.Bounds = new Rectangle(0, 0, 1, 1);
            autoHideCtrl.Visible = false;
            autoHideCtrl.ActiveContentChanged += (s, e) => OnActiveAutoHideContentChanged(e);
            BorderStyle = BorderStyle.FixedSingle;
            ShowAutoHideContentOnHover = true;
            Controls.Add(autoHideCtrl);
            Controls.Add(dummyControl);
            Controls.AddRange(dockWindows.dockWindows);
        }

        public bool AllowEndUserDocking { get; set; }
        public bool AllowEndUserNestedDocking { get; set; }
        internal DockDragHandler DockDragHandler { get; private set; }

        FormDockContent dummyContent = new FormDockContent();
        internal FormDockContent DummyContent
        {
            get { return dummyContent; }
        }
        DockPanelExtender extender = new DockPanelExtender();
        [Browsable(false)]
        public DockPanelExtender Extender
        {
            get { return extender; }
        }
        internal IDockGridFactory DockGridFactory
        {
            get { return extender.DockGridFactory; }
        }
        internal IFloatWindowFactory FloatWindowFactory
        {
            get { return extender.FloatWindowFactory; }
        }
        internal IDockGridCaptionFactory DockGridCaptionFactory
        {
            get { return extender.DockGridCaptionFactory; }
        }
        internal IDockGridStripFactory DockGridStripFactory
        {
            get { return extender.DockGridStripFactory; }
        }
        internal IAutoHideStripFactory AutoHideStripFactory
        {
            get { return extender.AutoHideStripFactory; }
        }
        internal void BeginDrag(ISplitterDragSource dragSource, Rectangle splitterScreenBounds)
        {
            splitterDragHandler.BeginDrag(dragSource, splitterScreenBounds);
        }
        internal void BeginDrag(IDockDragSource dragSource)
        {
            DockDragHandler.BeginDrag(dragSource);
        }
        public IContentFocusManager ContentFocusManager
        {
            get { return focusManager; }
        }
        internal AutoHideWindow AutoHideControl
        {
            get { return autoHideCtrl; }
        }
        AutoHideStripBase autoHideStrip;
        internal AutoHideStripBase AutoHideStrip
        {
            get
            {
                if (autoHideStrip == null)
                {
                    autoHideStrip = AutoHideStripFactory.CreateAutoHideStrip(this);
                    Controls.Add(autoHideStrip);
                }
                return autoHideStrip;
            }
        }


        public DockGrid GetGridAtCursor()
        {
            return GetGridAtPoint(Cursor.Position);
        }
        public DockGrid GetGridAtPoint(Point screenPoint)
        {
            var handler = User32.WindowFromPoint(screenPoint);
            var ctrl = Control.FromChildHandle(handler);
            while (ctrl != null)
            {
                var content = ctrl as IDockContent;
                if (content != null && content.Handler.DockPanel == this)
                    return content.Handler.DockGrid;
                var grid = ctrl as DockGrid;
                if (grid != null && grid.DockPanel == this)
                    return grid;
                ctrl = ctrl.Parent;
            }
            return null;
        }
        public FloatWindow GetFloatWindowAtCursor()
        {
            return GetFloatWindowAtPoint(Cursor.Position);
        }
        public FloatWindow GetFloatWindowAtPoint(Point screenPoint)
        {
            var ctrl = this.GetChildAtPoint(screenPoint);
            while (ctrl != null)
            {
                if (ctrl is FloatWindow)
                    return ctrl as FloatWindow;
            }
            return null;
        }

        DockPanelSkin skin = new DockPanelSkin();
        [Category("Docking")]
        [Description("DockPanel皮肤")]
        public DockPanelSkin Skin
        {
            get { return skin; }
            set { skin = value; }
        }

        Size defaultFloatWindowSize = new Size(300, 300);
        [Category("Docking"), Description("浮动窗口默认大小"), DefaultValue(typeof(Size), "300,300")]
        public Size DefaultFloatWindowSize
        {
            get { return defaultFloatWindowSize; }
            set { defaultFloatWindowSize = value; }
        }

        internal DockWindowCollection DockWindows { get { return dockWindows; } }

        [Category("Docking")]
        [Description("LeftDockPortion")]
        [DefaultValue(0.25)]
        public double DockPortionLeft
        {
            get { return DockWindows[DockStyle.Left].AutoHidePortion; }
            set { DockWindows.SetDockPortion(DockStyle.Left, value); }
        }
        [Category("Docking")]
        [Description("DockPortionRight")]
        [DefaultValue(0.25)]
        public double DockPortionRight
        {
            get { return DockWindows[DockStyle.Right].AutoHidePortion; }
            set { DockWindows.SetDockPortion(DockStyle.Right, value); }
        }
        [Category("Docking")]
        [Description("DockPortionTop")]
        [DefaultValue(0.25)]
        public double DockPortionTop
        {
            get { return DockWindows[DockStyle.Top].AutoHidePortion; }
            set { DockWindows.SetDockPortion(DockStyle.Top, value); }
        }
        [Category("Docking")]
        [Description("DockPortionBottom")]
        [DefaultValue(0.25)]
        public double DockPortionBottom
        {
            get { return DockWindows[DockStyle.Bottom].AutoHidePortion; }
            set { DockWindows.SetDockPortion(DockStyle.Bottom, value); }
        }

        bool documentIconVisible = false;
        [DefaultValue(false)]
        [Category("Docking")]
        [Description("获取或设置文档图标是否显示")]
        public bool DocumentIconVisible
        {
            get { return documentIconVisible; }
            set
            {
                if (documentIconVisible == value)
                    return;
                documentIconVisible = value;
                Refresh();
            }
        }
        [Category("Docking")]
        [Description("获取或设置鼠标悬停时是否显示隐藏内容")]
        [DefaultValue(true)]
        public bool ShowAutoHideContentOnHover { get; set; }

        /// <summary>
        /// 获取可悬停的整个DockPanel区域
        /// </summary>
        internal Rectangle DockBounds
        {
            get
            {
                return new Rectangle(DockPadding.Left, DockPadding.Top,
                    ClientRectangle.Width - DockPadding.GetHorizontal(),
                    ClientRectangle.Height - DockPadding.GetVertical());
            }
        }
        /// <summary>
        /// 获取可悬停的Document区域
        /// </summary>
        internal Rectangle DocumentDockBounds
        {
            get
            {
                var rectDocumentBounds = DisplayRectangle;
                if (DockWindows[DockStyle.Left].Visible)
                {
                    rectDocumentBounds.X += DockWindows[DockStyle.Left].Width;
                    rectDocumentBounds.Width -= DockWindows[DockStyle.Left].Width;
                }
                if (DockWindows[DockStyle.Right].Visible)
                    rectDocumentBounds.Width -= DockWindows[DockStyle.Right].Width;
                if (DockWindows[DockStyle.Top].Visible)
                {
                    rectDocumentBounds.Y += DockWindows[DockStyle.Top].Height;
                    rectDocumentBounds.Height -= DockWindows[DockStyle.Top].Height;
                }
                if (DockWindows[DockStyle.Bottom].Visible)
                    rectDocumentBounds.Height -= DockWindows[DockStyle.Bottom].Height;

                return rectDocumentBounds;

            }
        }
        internal Rectangle AutoHideCtrlBounds
        {
            get
            {
                if (ActiveAutoHideContent == null || Parent == null)
                    return Rectangle.Empty;

                var autoHideSize = 0;
                var rect = DockBounds;
                var portion = ActiveAutoHideContent.Handler.AutoHidePortion;
                switch (autoHideCtrl.Dock)
                {
                    case DockStyle.Left:
                    case DockStyle.Right:
                        if (portion < 1)
                            portion *= rect.Width;
                        autoHideSize = (int)Math.Min(portion, rect.Width - Const.MinGridSize);
                        break;
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        if (portion < 1)
                            portion *= rect.Height;
                        autoHideSize = (int)Math.Min(portion, rect.Height - Const.MinGridSize);
                        break;
                }

                switch (autoHideCtrl.Dock)
                {
                    case DockStyle.Left:
                        rect.Width = autoHideSize;
                        break;
                    case DockStyle.Right:
                        rect.X = rect.Right - autoHideSize;
                        rect.Width = autoHideSize;
                        break;
                    case DockStyle.Top:
                        rect.Height = autoHideSize;
                        break;
                    case DockStyle.Bottom:
                        rect.Y = rect.Bottom - autoHideSize;
                        rect.Height = autoHideSize;
                        break;
                    default:
                        rect = Rectangle.Empty;
                        break;
                }
                return rect;
            }
        }
        [Browsable(false)]
        public IDockContent ActiveAutoHideContent
        {
            get { return autoHideCtrl.ActiveContent; }
            set { autoHideCtrl.ActiveContent = value; }
        }
        [Browsable(false)]
        public IDockContent ActiveContent
        {
            get { return focusManager.ActiveContent; }
        }

        [Browsable(false)]
        public DockGrid ActiveGrid
        {
            get { return focusManager.ActiveGrid; }
        }

        [Browsable(false)]
        public IDockContent ActiveDocument
        {
            get { return focusManager.ActiveDocument; }
        }

        [Browsable(false)]
        public DockGrid ActiveDocumentGrid
        {
            get { return focusManager.ActiveDocumentGrid; }
        }
        public new void SuspendLayout()
        {
            focusManager.SuspendFocus();
            base.SuspendLayout();
        }
        public new void ResumeLayout(bool performLayout)
        {
            focusManager.ResumeFocus();
            base.ResumeLayout(performLayout);
        }
        bool rightToLeftLayout = false;
        [DefaultValue(false)]
        [Category("Appearance")]
        [Description("RightToLeftLayout")]
        public bool RightToLeftLayout
        {
            get { return rightToLeftLayout; }
            set
            {
                if (rightToLeftLayout == value)
                    return;
                rightToLeftLayout = value;
                foreach (var floatWindow in floatWindows)
                    floatWindow.RightToLeftLayout = value;
            }
        }

        List<DockGrid> grids = new List<DockGrid>();
        public IList<DockGrid> Grids { get { return grids; } }
        internal void AddGrid(DockGrid grid)
        {
            ParamGuard.NotNull(grid, "grid");
            if (grids.Contains(grid))
                return;
            grids.Add(grid);
        }
        internal void RemoveGrid(DockGrid grid)//因为添加时检查了grid不为空，所以在删除时不需要再次进行检查
        {
            grids.Remove(grid);
        }

        HashSet<FloatWindow> floatWindows = new HashSet<FloatWindow>();
        internal void AddFloatWindow(FloatWindow floatWindow)
        {
            ParamGuard.NotNull(floatWindow, "floatWindow");
            floatWindows.Add(floatWindow);
        }
        internal void RemoveFloatWindow(FloatWindow floatWindow)
        {
            if (floatWindows.Remove(floatWindow) && floatWindows.Count == 0)
                this.FindForm().Focus();
        }

        HashSet<IDockContent> contents = new HashSet<IDockContent>();
        internal void AddContent(IDockContent content)
        {
            ParamGuard.NotNull(content, "content");
            if (contents.Add(content))
                OnContentAdded(new DockContentEventArgs(content));
        }
        internal void RemoveContent(IDockContent content)
        {
            if (contents.Remove(content))
                OnContentRemoved(new DockContentEventArgs(content));
        }

        sealed protected override void OnLayout(LayoutEventArgs levent)
        {
            SuspendLayout();
            AutoHideStrip.Bounds = ClientRectangle;
            SetDockPadding();
            SetDockWindowsWidth();
            autoHideCtrl.Bounds = AutoHideCtrlBounds;
            DockWindows[DockStyle.Fill].BringToFront();
            autoHideCtrl.BringToFront();

            base.OnLayout(levent);

            InvalidateWindowRegion();
            ResumeLayout(true);
        }
        void SetDockPadding()
        {
            DockPadding.All = 0;
            var height = AutoHideStrip.StripHeight;
            if (AutoHideStrip.GetGrids(DockStyle.Left).Any())
                DockPadding.Left = height;
            if (AutoHideStrip.GetGrids(DockStyle.Right).Any())
                DockPadding.Right = height;
            if (AutoHideStrip.GetGrids(DockStyle.Top).Any())
                DockPadding.Top = height;
            if (AutoHideStrip.GetGrids(DockStyle.Bottom).Any())
                DockPadding.Bottom = height;
        }
        void SetDockWindowsWidth()
        {
            DockWindows[DockStyle.Left].Width = dockWindows.GetDockWindowSize(DockStyle.Left);
            DockWindows[DockStyle.Right].Width = dockWindows.GetDockWindowSize(DockStyle.Right);
            DockWindows[DockStyle.Top].Height = dockWindows.GetDockWindowSize(DockStyle.Top);
            DockWindows[DockStyle.Bottom].Height = dockWindows.GetDockWindowSize(DockStyle.Bottom);
        }
        internal void SaveFocus()
        {
            dummyControl.Focus();
        }
        void InvalidateWindowRegion()
        {
            if (DesignMode)
                return;
            dummyControl.Paint += DummyControl_Paint;
            dummyControl.Refresh();
        }
        void DummyControl_Paint(object sender, PaintEventArgs e)
        {
            dummyControl.Paint -= DummyControl_Paint;
        }
        public void UpdateDockWindowZOrder(DockStyle dockStyle, bool fullPanelEdge)
        {
            if (fullPanelEdge)
                DockWindows[dockStyle].SendToBack();
            else
                DockWindows[dockStyle].BringToFront();
        }
        internal void RefreshAutoHideStrip()
        {
            AutoHideStrip.RefreshChanges();
        }
        internal void RefreshActiveAutoHideContent()
        {
            autoHideCtrl.RefreshActiveContent();
        }

        [Category("Docking")]
        [Description("DockPanel_ContentRemoved_Description")]
        public event EventHandler<DockContentEventArgs> ContentAdded;
        protected virtual void OnContentAdded(DockContentEventArgs e)
        {
            if (ContentAdded != null)
                ContentAdded(this, e);
        }
        [Category("Docking")]
        [Description("DockPanel_ContentRemoved_Description")]
        public event EventHandler<DockContentEventArgs> ContentRemoved;
        protected virtual void OnContentRemoved(DockContentEventArgs e)
        {
            if (ContentRemoved != null)
                ContentRemoved(this, e);
        }
        [Category("PropertyChanged")]
        [Description("DockPanel_ActiveDocumentChanged_Description")]
        public event EventHandler ActiveDocumentChanged;
        protected virtual void OnActiveDocumentChanged(EventArgs e)
        {
            if (ActiveDocumentChanged != null)
                ActiveDocumentChanged(this, e);
        }
        internal void RaiseActiveDocumentChanged(EventArgs e)
        {
            OnActiveDocumentChanged(e);
        }
        [Category("Docking")]
        [Description("DockPanel_ActiveAutoHideContentChanged_Description")]
        public event EventHandler ActiveAutoHideContentChanged;
        protected virtual void OnActiveAutoHideContentChanged(EventArgs e)
        {
            if (ActiveAutoHideContentChanged != null)
                ActiveAutoHideContentChanged(this, e);
        }
        [Category("PropertyChanged")]
        [Description("DockPanel_ActiveContentChanged_Description")]
        public event EventHandler ActiveContentChanged;
        protected void OnActiveContentChanged(EventArgs e)
        {
            if (ActiveContentChanged != null)
                ActiveContentChanged(this, e);
        }
        internal void RaiseActiveContentChanged(EventArgs e)
        {
            OnActiveContentChanged(e);
        }
        [Category("PropertyChanged")]
        [Description("DockPanel_ActivePaneChanged_Description")]
        public event EventHandler ActiveGridChanged;
        protected virtual void OnActiveGridChanged(EventArgs e)
        {
            if (ActiveGridChanged != null)
                ActiveGridChanged(this, e);
        }
        internal void RaiseActiveGridChanged(EventArgs e)
        {
            OnActiveGridChanged(e);
        }
    }
}
