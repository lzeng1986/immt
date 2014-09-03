using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 提供默认的<see cref="DockGridCaptionBase"/>实现，表示标题栏，只有停靠在四周的DockWindow才有标题栏
    /// </summary>
    public class DockGridCaption : DockGridCaptionBase
    {
        static LazyLoader<Blend> ActiveBackColorGradientBlend = LazyLoader.New(
            () => new Blend(2)
            {
                Factors = new[] { 0.5F, 1.0F },
                Positions = new[] { 0.0F, 1.0F }
            });

        protected static readonly Padding TextMargin = new Padding(3, 2, 3, 3);
        protected static readonly Padding ButtonMargin = new Padding(1, 2, 1, 2);

        protected static readonly Bitmap AutoHideBmp = ControlRes.AutoHide;
        protected static readonly Bitmap DockBmp = ControlRes.Dock;
        protected static readonly Bitmap CloseBmp = ControlRes.Close;
        protected static readonly Bitmap OptionBmp = ControlRes.Option;

        const int ButtonGap = 1;

        IContainer components;
        ToolTip toolTip;
        ButtonBase optionsButton;
        ButtonBase closeButton;
        ButtonBase autoHideButton;

        public DockGridCaption(DockGrid grid)
            : base(grid)
        {
            SuspendLayout();

            components = new Container();
            toolTip = new ToolTip(components);

            optionsButton = new SimpleButton(OptionBmp);
            toolTip.SetToolTip(optionsButton, "选项");
            optionsButton.MouseClick += new MouseEventHandler(optionsButton_MouseClick);

            closeButton = new SimpleButton(CloseBmp);
            toolTip.SetToolTip(closeButton, "关闭");
            closeButton.MouseClick += new MouseEventHandler(closeButton_MouseClick);

            autoHideButton = new AutoHideButton(this, DockBmp, OptionBmp);
            toolTip.SetToolTip(autoHideButton, "自动隐藏");
            autoHideButton.MouseClick += new MouseEventHandler(autoHideButton_MouseClick);

            this.Controls.AddRange(new[] { optionsButton, closeButton, autoHideButton });

            ResumeLayout();

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components.Dispose();
            base.Dispose(disposing);
        }
        void autoHideButton_MouseClick(object sender, MouseEventArgs e)
        {
            DockGrid.IsAutoHide = !DockGrid.IsAutoHide;
            if (DockGrid.IsAutoHide)
            {
                DockGrid.DockPanel.ActiveAutoHideContent = null;
                //DockGrid.NestedGrids.SwitchGridWithFirstChild(DockGrid);
            }
        }

        void closeButton_MouseClick(object sender, MouseEventArgs e)
        {
            DockGrid.CloseActiveContent();
        }

        void optionsButton_MouseClick(object sender, MouseEventArgs e)
        {
            DockGrid.ShowTabPageContextMenu(this, PointToClient(Cursor.Position));
        }

        internal bool IsAutoHide
        {
            get { return DockGrid.IsAutoHide; }
        }
        protected internal override int CaptionHeight
        {
            get { return Math.Max(TextFont.Height + TextMargin.Vertical, closeButton.Bmp.Height + TextMargin.Vertical); }
        }
        bool CloseButtonEnabled
        {
            get { return (DockGrid.ActiveContent != null) ? DockGrid.ActiveContent.Handler.CloseButtonEnabled : false; }
        }

        bool CloseButtonVisible
        {
            get { return (DockGrid.ActiveContent != null) ? DockGrid.ActiveContent.Handler.CloseButtonVisible : false; }
        }

        bool AutoHideButtonVisible
        {
            get { return !DockGrid.IsFloat; }
        }
        Font TextFont
        {
            get { return DockGrid.DockPanel.StripFont; }
        }

        const TextFormatFlags textFormat = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
        TextFormatFlags TextFormat
        {
            get
            {
                if (RightToLeft == RightToLeft.No)
                    return textFormat;
                else
                    return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawCaption(e.Graphics);
        }
        void DrawCaption(Graphics g)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
                return;

            if (DockGrid.IsActivated)
            {
                g.FillRectangle(SystemBrushes.ActiveCaption, ClientRectangle);
            }
            else
            {
                g.FillRectangle(SystemBrushes.InactiveCaption, ClientRectangle);
            }

            var rectCaption = ClientRectangle;
            var rectCaptionText = rectCaption;
            rectCaptionText.X += TextMargin.Left;
            rectCaptionText.Width -= TextMargin.Horizontal + ButtonMargin.Horizontal + closeButton.Width;
            if (AutoHideButtonVisible)
                rectCaptionText.Width -= autoHideButton.Width + ButtonGap;
            if (HasTabPageContextMenu)
                rectCaptionText.Width -= optionsButton.Width + ButtonGap;
            rectCaptionText.Y += TextMargin.Top;
            rectCaptionText.Height -= TextMargin.Vertical;

            var textColor = DockGrid.IsActivated ? SystemColors.ActiveCaptionText : SystemColors.InactiveCaptionText;
            TextRenderer.DrawText(g, DockGrid.CaptionText, TextFont, this.RtlTransformRect(rectCaptionText), textColor, TextFormat);
        }
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }
        protected override void OnLayout(LayoutEventArgs levent)
        {
            SetButtonsPosition();
            base.OnLayout(levent);
        }

        protected override void OnRefreshChanges()
        {
            SetButtons();
            Invalidate();
        }
        void SetButtons()
        {
            closeButton.Enabled = CloseButtonEnabled;
            closeButton.Visible = CloseButtonVisible;
            autoHideButton.Visible = AutoHideButtonVisible;
            optionsButton.Visible = HasTabPageContextMenu;
            closeButton.RefreshChanges();
            autoHideButton.RefreshChanges();
            optionsButton.RefreshChanges();
            SetButtonsPosition();
        }
        void SetButtonsPosition()
        {
            Rectangle rectCaption = ClientRectangle;
            int buttonWidth = closeButton.Bmp.Width;
            int buttonHeight = closeButton.Bmp.Height;
            int height = ClientSize.Height - ButtonMargin.Vertical - 2;
            if (height < 4)
                height = 4;
            if (buttonHeight > height)
            {
                buttonWidth = buttonWidth * (height / buttonHeight);
                buttonHeight = height;
            }
            var buttonSize = new Size(buttonWidth, buttonHeight);
            int x = rectCaption.Right - 1 - ButtonMargin.Right - buttonWidth;
            int y = rectCaption.Top + ButtonMargin.Top;
            var point = new Point(x, y);
            closeButton.Bounds = this.RtlTransformRect(new Rectangle(point, buttonSize));

            if (CloseButtonVisible)
                point.Offset(-(buttonWidth + ButtonGap), 0);

            autoHideButton.Bounds = this.RtlTransformRect(new Rectangle(point, buttonSize));
            if (AutoHideButtonVisible)
                point.Offset(-(buttonWidth + ButtonGap), 0);
            optionsButton.Bounds = this.RtlTransformRect(new Rectangle(point, buttonSize));
        }
    }
    
}
