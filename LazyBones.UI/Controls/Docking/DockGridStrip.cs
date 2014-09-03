using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 表示一个DockGrid的多文档标签页控件，当显示为文档时标签在上方，其余情况则标签在下方
    /// </summary>
    public class DockGridStrip : DockGridStripBase
    {
        static readonly Padding StripPadding = new Padding(0, 0, 0, 1);
        static readonly Padding ToolWindowTabSeperatorMargin = new Padding(0, 3, 0, 3);

        const int TabMaxWidth = 200;
        const int DocumentButtonGap = 0;
        static readonly Padding ButtonMargin = new Padding(0, 4, 3, 4);
        static readonly Padding TabPadding = new Padding(3, 3, 3, 0);
        const int TextGap = 3;
        static readonly Padding IconMargin = new Padding(2, 3, 0, 1);
        static readonly Size IconSize = new Size(16, 16);

        static Pen ToolWindowTabBorderPen = SystemPens.GrayText;
        static Pen DocumentTabActiveBorderPen = new Pen(Color.Gold, TabPadding.Bottom);
        static Pen DocumentTabInactiveBorderPen = SystemPens.GrayText;

        

        ContextMenuStrip selectMenu;
        protected static readonly Bitmap CloseBmp = ControlRes.Close;
        protected static readonly Bitmap WindowListBmp = ControlRes.Option;
        protected static readonly Bitmap WindowListOverflowBmp = ControlRes.OptionOverflow;
        SimpleButton closeButton;
        ListButton windowListButton;
        IContainer components;
        ToolTip toolTip;

        public DockGridStrip(DockGrid dockGrid)
            : base(dockGrid)
        {
            SuspendLayout();

            components = new Container();
            toolTip = new ToolTip(components);

            closeButton = new SimpleButton(CloseBmp);
            toolTip.SetToolTip(closeButton, "关闭");
            closeButton.MouseClick += delegate { DockGrid.CloseActiveContent(); };

            windowListButton = new ListButton(WindowListBmp, WindowListOverflowBmp);
            toolTip.SetToolTip(windowListButton, "窗体列表");
            windowListButton.MouseClick += new MouseEventHandler(windowListButton_MouseClick);
            this.Controls.AddRange(new Control[] { closeButton, windowListButton });
            selectMenu = new ContextMenuStrip(components);
            ResumeLayout();
        }

        void windowListButton_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (ToolStripItem item in selectMenu.Items)
            {
                item.Click -= item_Click;
            }
            selectMenu.Items.Clear();
            foreach (var tab in Tabs)
            {
                var content = tab.Content;
                var item = selectMenu.Items.Add(content.Handler.TabText, content.Handler.Icon.ToBitmap());
                item.Tag = tab.Content;
                item.Click += item_Click;
            }
            selectMenu.Show(windowListButton, e.Location);
        }

        void item_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item != null)
            {
                DockGrid.ActiveContent = (IDockContent)item.Tag;
            }
        }
        void SetButtons()
        {
            if (Appearance == AppearanceStyle.ToolWindow)
            {
                if (closeButton != null)
                    closeButton.Left = -closeButton.Width;

                if (windowListButton != null)
                    windowListButton.Left = -windowListButton.Width;
            }
            else
            {
                closeButton.Enabled = DockGrid.ActiveContent == null ? true : DockGrid.ActiveContent.Handler.CloseButtonEnabled;
                closeButton.Visible = DockGrid.ActiveContent == null ? true : DockGrid.ActiveContent.Handler.CloseButtonVisible;
                closeButton.RefreshChanges();
                windowListButton.RefreshChanges();
            }
        }
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (Appearance == AppearanceStyle.Document)
            {
                SetButtonsPosition();
                OnRefreshChanges();
            }

            base.OnLayout(levent);
        }

        void SetButtonsPosition()
        {
            var rectTabStrip = DisplayRectangle;

            var buttonWidth = closeButton.Bmp.Width;
            var buttonHeight = closeButton.Bmp.Height;
            var height = rectTabStrip.Height - ButtonMargin.Vertical - 2;
            if (height < 4)
                height = 4;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * (height / buttonHeight);
                buttonHeight = height;
            }
            Size buttonSize = new Size(buttonWidth, buttonHeight);

            int x = rectTabStrip.Right - TabPadding.Horizontal - buttonWidth;
            int y = rectTabStrip.Top + TabPadding.Top;
            Point point = new Point(x, y);
            closeButton.Bounds = this.RtlTransformRect(new Rectangle(point, buttonSize));

            if (closeButton.Visible)
                point.Offset(-(DocumentButtonGap + buttonWidth), 0);

            windowListButton.Bounds = this.RtlTransformRect(new Rectangle(point, buttonSize));
        }
        public override Rectangle DisplayRectangle//该strip显示的边界
        {
            get
            {
                var rect = ClientRectangle;
                rect.X += StripPadding.Left;
                rect.Width -= StripPadding.Horizontal;
                rect.Y += StripPadding.Top;
                rect.Height -= StripPadding.Vertical;
                return rect;
            }
        }
        Rectangle TabsRectangle//显示所有tab的边界，当为Document风格时为TabStripRectangle除去按钮大小
        {
            get
            {
                var rect = DisplayRectangle;
                if (Appearance == AppearanceStyle.Document)
                {
                    rect.X += TabPadding.Left;
                    rect.Width -= TabPadding.Horizontal + windowListButton.Width + 2 * DocumentButtonGap;
                }
                return rect;
            }
        }
        Font TextFont
        {
            get { return DockGrid.DockPanel.StripFont; }
        }

        const TextFormatFlags textFormat = TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
        TextFormatFlags DocumentTextFormat
        {
            get
            {
                if (RightToLeft == RightToLeft.Yes)
                    return textFormat | TextFormatFlags.RightToLeft;
                return textFormat;
            }
        }
        TextFormatFlags ToolWindowTextFormat
        {
            get
            {
                if (RightToLeft == RightToLeft.Yes)
                    return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                return textFormat;
            }
        }
        bool DocumentTabsOverflow
        {
            get { return windowListButton.Overflowed; }
            set
            {
                if (windowListButton.Overflowed == value)
                    return;
                windowListButton.Overflowed = value;
            }
        }
        protected internal override void EnsureTabVisible(IDockContent content)
        {
            if (Appearance != AppearanceStyle.Document || !Tabs.Contains(content))
                return;

            CalculateTabs();
            EnsureDocumentTabVisible(content, true);
        }
        bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
        {
            int index = Tabs.IndexOf(content);
            var tab = Tabs[index];
            if (tab.DisplayWidth != 0)
                return false;

            displayingTabIndex = index;
            if (repaint)
                Invalidate();
            return true;
        }
        int GetTabRequiredWidth(StripTab tab)//(图标宽度+图标边缘空隙)+文字宽度
        {
            var textSize = TextRenderer.MeasureText(tab.Content.Handler.TabText, TextFont);
            var width = textSize.Width + TextGap;
            if (Appearance == AppearanceStyle.ToolWindow || DockGrid.DockPanel.DocumentIconVisible)
                width += IconSize.Width + IconMargin.Horizontal;
            return Math.Min(width, TabMaxWidth);
        }
        void DrawTabStrip(Graphics g)
        {
            if (Appearance == AppearanceStyle.Document)
                DrawTabStripInDocument(g);
            else
                DrawTabStripInToolWindow(g);
        }

        void DrawTabStripInDocument(Graphics g)
        {
            if (!Tabs.Any())
                return;
            var rectTabOnly = TabsRectangle;
            g.SetClip(this.RtlTransformRect(rectTabOnly));
            StripTab activeTab = null;
            Rectangle activeRect = Rectangle.Empty;
            foreach (var item in Tabs.Select(t => new { tab = t, rect = GetTabBounds(t) }))
            {
                if (item.tab.Content == DockGrid.ActiveContent)
                {
                    activeTab = item.tab;
                    activeRect = item.rect;
                }
                else if (item.rect.IntersectsWith(rectTabOnly))
                {
                    DrawTab(g, item.tab, item.rect);
                }
            }

            var rectTabStrip = DisplayRectangle;
            g.SetClip(rectTabStrip);
            g.DrawLine(DocumentTabActiveBorderPen, rectTabStrip.Left, rectTabStrip.Bottom, rectTabStrip.Right, rectTabStrip.Bottom);
            g.SetClip(this.RtlTransformRect(rectTabOnly));

            if (activeTab != null && activeRect.IntersectsWith(rectTabOnly))
                DrawTab(g, activeTab, activeRect);
        }

        void DrawTabStripInToolWindow(Graphics g)
        {
            var rectTabStrip = DisplayRectangle;
            g.DrawLine(ToolWindowTabBorderPen, rectTabStrip.Left, rectTabStrip.Top, rectTabStrip.Right, rectTabStrip.Top);
            foreach (var tab in Tabs)
                DrawTab(g, tab, GetTabBounds(tab));
        }
        Rectangle GetTabBounds(StripTab tab)
        {
            var rect = DisplayRectangle;
            rect.X = tab.Left;
            rect.Width = tab.DisplayWidth;
            return rect;
        }
        void CalculateTabs()
        {
            if (Appearance == AppearanceStyle.ToolWindow)
                CalculateTabsInToolWindow();
            else
                CalculateTabsInDocument();
        }
        void CalculateTabsInToolWindow()//所有标签页均显示，如果只有一个标签，则不显示标签栏
        {
            int tabsCount = Tabs.Count;
            if (tabsCount <= 1 || DockGrid.IsAutoHide)
                return;
            var tabs = Tabs.ToList();
            tabs.ForEach(t => t.RequiredWidth = GetTabRequiredWidth(t));

            var tabStripRect = DisplayRectangle;
            int totalWidth = tabStripRect.Width;
            int totalAllocatedWidth = 0;

            while (tabs.Count > 0)
            {
                var avgWidth = totalWidth / tabs.Count;
                bool anyWidthLessThanAvg = false;
                foreach (var tab in tabs.Where(t => t.RequiredWidth <= avgWidth))
                {
                    tab.DisplayWidth = tab.RequiredWidth;
                    totalAllocatedWidth += tab.DisplayWidth;
                    anyWidthLessThanAvg = true;
                }
                if (!anyWidthLessThanAvg)
                    break;
                tabs.RemoveAll(t => t.RequiredWidth <= avgWidth);
            }
            tabs.Clear();
            var left = tabStripRect.Left;
            foreach (var tab in Tabs)
            {
                tab.Left = left;
                left += tab.DisplayWidth;
            }
        }
        int displayingTabIndex = 0;
        void CalculateTabsInDocument()//需要保证当前激活标签显示
        {
            var tabsCount = Tabs.Count;
            if (displayingTabIndex >= tabsCount)
                displayingTabIndex = 0;

            foreach (var tab in Tabs)
                tab.RequiredWidth = GetTabRequiredWidth(tab);

            var tabStripRect = TabsRectangle;

            var left = tabStripRect.Left + tabStripRect.Height / 2;
            bool overflow = false;

            int tempLeft = left;
            var startTab = Tabs[displayingTabIndex];
            startTab.RequiredWidth = GetTabRequiredWidth(startTab);

            tempLeft = CalculateTabWidthInDocument(tabStripRect.Right, tempLeft, startTab);

            for (int i = displayingTabIndex - 1; i >= 0; i--)
                tempLeft = CalculateTabWidthInDocument(tabStripRect.Right, tempLeft, Tabs[i]);

            overflow |= tempLeft > tabStripRect.Right;

            for (int i = displayingTabIndex + 1; i < tabsCount; i++)
                tempLeft = CalculateTabWidthInDocument(tabStripRect.Right, tempLeft, Tabs[i]);

            overflow |= tempLeft > tabStripRect.Right;

            left = tabStripRect.X;
            foreach (var tab in Tabs)
            {
                tab.Left = left;
                left += tab.DisplayWidth;
            }
            DocumentTabsOverflow = overflow;
        }
        int CalculateTabWidthInDocument(int rightBound, int left, StripTab tab)
        {
            left += tab.RequiredWidth;
            if (left < rightBound)
                tab.DisplayWidth = tab.RequiredWidth;
            else
                tab.DisplayWidth = 0;
            return left;
        }
        GraphicsPath GetTabOutline(StripTab tab, bool rtlTransform, bool toScreen)
        {
            var tabBounds = GetTabBounds(tab);
            if (rtlTransform)
                tabBounds = this.RtlTransformRect(tabBounds);
            if (toScreen)
                tabBounds = RectangleToScreen(tabBounds);
            if (Appearance == AppearanceStyle.ToolWindow)
                return tabBounds.GetDownRoundCorner();
            else
                return tabBounds.GetUpRoundCorner();
        }

        void DrawTab(Graphics g, StripTab tab, Rectangle tabBounds)
        {
            Rectangle iconRect = new Rectangle(
                tabBounds.Left + IconMargin.Left,
                tabBounds.Top + IconMargin.Top,
                IconSize.Width, IconSize.Height);
            Rectangle textRect = iconRect;
            textRect.X += iconRect.Width + IconMargin.Right;
            textRect.Y = tabBounds.Y;
            textRect.Height = tabBounds.Height;

            if (Appearance == AppearanceStyle.Document && DockGrid.DockPanel.DocumentIconVisible)
                textRect.Width = tabBounds.Width - iconRect.Width - IconMargin.Horizontal - TextGap;
            else
                textRect.Width = tabBounds.Width - IconMargin.Left - TextGap;

            tabBounds = this.RtlTransformRect(tabBounds);
            textRect = this.RtlTransformRect(textRect);
            iconRect = this.RtlTransformRect(iconRect);

            var path = GetTabOutline(tab, true, false);

            Color textColor;
            Brush brush;
            if (DockGrid.ActiveContent == tab.Content)
            {
                brush = SystemBrushes.GradientActiveCaption;
                textColor = SystemColors.GradientActiveCaption;
            }
            else
            {
                brush = SystemBrushes.GradientInactiveCaption;
                textColor = SystemColors.GradientInactiveCaption;
            }
            using (brush) { g.FillPath(brush, path); }
            if (Appearance == AppearanceStyle.ToolWindow)//绘制分割线
                g.DrawLine(SystemPens.ControlDarkDark, tabBounds.Right, tabBounds.Top, tabBounds.Right, tabBounds.Bottom);

            if (DockGrid.ActiveContent == tab.Content)
                g.DrawPath(SystemPens.GrayText, path);

            TextRenderer.DrawText(g, tab.Content.Handler.TabText, TextFont, textRect, textColor, ToolWindowTextFormat);

            if (Appearance == AppearanceStyle.ToolWindow || DockGrid.DockPanel.DocumentIconVisible)
                g.DrawIcon(tab.Content.Handler.Icon, iconRect);

            path.Dispose();
        }

        protected internal override GraphicsPath GetOutline(int index)//ToolWindow风格的标签在下方，Document风格的标签在上方
        {
            var tab = Tabs[index];
            if (Appearance == AppearanceStyle.ToolWindow)
                return GetOutlineInToolWindow(tab);
            else
                return GetOutlineInDocument(tab);
        }
        GraphicsPath GetOutlineInToolWindow(StripTab tab)
        {
            Rectangle tabRect = GetTabBounds(tab);
            tabRect = RectangleToScreen(this.RtlTransformRect(tabRect));
            Rectangle gridRect = DockGrid.RectangleToScreen(DockGrid.ClientRectangle);
            var path = GetTabOutline(tab, true, true);
            var points = new[] { 
                new Point(tabRect.Left, tabRect.Top),
                new Point(gridRect.Left, tabRect.Top),
                new Point(gridRect.Left, gridRect.Top),
                new Point(gridRect.Right, gridRect.Top),
                new Point(gridRect.Right, tabRect.Top),
                new Point(tabRect.Right, gridRect.Top)
            };
            path.AddLines(points);
            return path;
        }
        GraphicsPath GetOutlineInDocument(StripTab tab)
        {
            var tabRect = GetTabBounds(tab);
            tabRect = RectangleToScreen(this.RtlTransformRect(tabRect));
            Rectangle gridRect = DockGrid.RectangleToScreen(DockGrid.ClientRectangle);
            var path = GetTabOutline(tab, true, true);
            var points = new[] { 
                new Point(tabRect.Right, tabRect.Bottom),
                new Point(gridRect.Right, tabRect.Bottom),
                new Point(gridRect.Right, gridRect.Bottom),
                new Point(gridRect.Left, gridRect.Bottom),
                new Point(gridRect.Left, tabRect.Bottom),
                new Point(tabRect.Left, tabRect.Bottom)
            };
            path.AddLines(points);
            return path;
        }

        protected internal override int StripHeight
        {
            get { return Appearance == AppearanceStyle.ToolWindow ? StripHeightInToolWindow : StripHeightInDocument; }
        }
        int StripHeightInToolWindow
        {
            get
            {
                if (DockGrid.IsAutoHide || Tabs.Count <= 1)
                    return 0;
                return Math.Max(TextFont.Height, IconMargin.Vertical + IconSize.Height) + StripPadding.Vertical;
            }
        }

        int StripHeightInDocument
        {
            get { return Math.Max(TextFont.Height + TabPadding.Vertical, closeButton.Height + ButtonMargin.Vertical) + StripPadding.Vertical; }
        }

        protected internal override int HitTest(Point screenPoint)
        {
            var pos = PointToClient(screenPoint);
            if (!TabsRectangle.Contains(pos))
                return -1;
            var ind = 0;
            foreach (var tab in Tabs)
            {
                if (GetTabBounds(tab).Contains(pos))
                    return ind;
                ind++;
            }
            return -1;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.ControlDark, DisplayRectangle);

            base.OnPaint(e);

            CalculateTabs();
            if (Appearance == AppearanceStyle.Document && DockGrid.ActiveContent != null)
            {
                if (EnsureDocumentTabVisible(DockGrid.ActiveContent, false))
                    CalculateTabs();
            }

            DrawTabStrip(e.Graphics);
        }
        protected override void OnMouseHover(EventArgs e)//悬停显示tooltip
        {
            var index = HitTest();
            var text = string.Empty;

            base.OnMouseHover(e);

            if (index != -1)
            {
                var tab = Tabs[index];
                if (!string.IsNullOrEmpty(tab.Content.Handler.ToolTipText))
                    text = tab.Content.Handler.ToolTipText;
                else if (tab.RequiredWidth > tab.DisplayWidth)
                    text = tab.Content.Handler.TabText;
            }

            if (toolTip.GetToolTip(this) != text)
            {
                toolTip.Active = false;
                toolTip.SetToolTip(this, text);
                toolTip.Active = true;
            }

            // requires further tracking of mouse hover behavior,
            ResetMouseEventArgs();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }
    }
}
