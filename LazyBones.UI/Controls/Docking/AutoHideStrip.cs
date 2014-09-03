using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 提供默认的<see cref="AutoHideStripBase"/>实现，表示四周的自动隐藏条
    /// </summary>
    public class AutoHideStrip : AutoHideStripBase
    {
        static readonly Size ImgSize = new Size(16, 16);
        static readonly Padding ImgMargin = new Padding(4, 2, 2, 2);
        static readonly Padding TextMargin = Padding.Empty;
        static readonly Padding TabMargin = new Padding(4, 3, 0, 0);
        const int TabGap = 10;

        public Font TextFont
        {
            get { return DockPanel.StripFont; }
        }

        static StringFormat stringFormatTabHorizontal = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };
        StringFormat TabStringFormatHorizontal
        {
            get
            {
                if (RightToLeft == RightToLeft.Yes)
                    stringFormatTabHorizontal.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                else
                    stringFormatTabHorizontal.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                return stringFormatTabHorizontal;
            }
        }
        static StringFormat stringFormatTabVertical = new StringFormat()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.DirectionVertical,
            Trimming = StringTrimming.None
        };
        StringFormat TabStringFormatVertical
        {
            get
            {
                if (RightToLeft == RightToLeft.Yes)
                    stringFormatTabVertical.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                else
                    stringFormatTabVertical.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                return stringFormatTabVertical;
            }
        }

        static Matrix matrixIdentity = new Matrix();
        static bool IsVertical(DockStyle dockStyle)
        {
            return dockStyle == DockStyle.Left || dockStyle == DockStyle.Right;
        }

        public AutoHideStrip(DockPanel panel)
            : base(panel)
        {
            SetStyle(ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = SystemColors.ControlLight;
        }

        Rectangle GetLogicalTabStripRectangle(DockStyle dockStyle)
        {
            return GetLogicalTabStripRectangle(dockStyle, false);
        }

        Rectangle GetLogicalTabStripRectangle(DockStyle dockStyle, bool transformed)
        {
            var hasLeftGrids = GridsOnLeft.Any();
            var hasTopGrids = GridsOnTop.Any();
            var hasRightGrids = GridsOnRight.Any();
            var hasBottomGrids = GridsOnBottom.Any();

            int x, y, width, height;
            height = StripHeight;
            if (dockStyle == DockStyle.Left && hasLeftGrids)
            {
                x = 0;
                y = hasTopGrids ? height : 0;
                width = Height - (hasTopGrids ? height : 0) - (hasBottomGrids ? height : 0);
            }
            else if (dockStyle == DockStyle.Right && hasRightGrids)
            {
                x = Width - height;
                if (hasLeftGrids && x < height)
                    x = height;
                y = hasTopGrids ? height : 0;
                width = Height - (hasTopGrids ? height : 0) - (hasBottomGrids ? height : 0);
            }
            else if (dockStyle == DockStyle.Top && hasTopGrids)
            {
                x = hasLeftGrids ? height : 0;
                y = 0;
                width = Width - (hasLeftGrids ? height : 0) - (hasRightGrids ? height : 0);
            }
            else if (dockStyle == DockStyle.Bottom && hasBottomGrids)
            {
                x = hasLeftGrids ? height : 0;
                y = Height - height;
                if (hasTopGrids && y < height)
                    y = height;
                width = Width - (hasLeftGrids ? height : 0) - (hasRightGrids ? height : 0);
            }
            else
                return Rectangle.Empty;

            if (width == 0 || height == 0)
            {
                return Rectangle.Empty;
            }

            var rect = new Rectangle(x, y, width, height);
            return transformed ? GetTransformedRectangle(dockStyle, rect) : rect;
        }
        Rectangle GetTransformedRectangle(DockStyle dockStyle, Rectangle rect)  //当停靠在左、右边时，旋转90°
        {
            if (!IsVertical(dockStyle))
                return rect;

            PointF[] pts = new PointF[1];
            pts[0].X = (float)rect.X + (float)rect.Width / 2;
            pts[0].Y = (float)rect.Y + (float)rect.Height / 2;
            var rectTabStrip = GetLogicalTabStripRectangle(dockStyle);
            var matrix = new Matrix();
            matrix.RotateAt(90, new PointF(rectTabStrip.X + rectTabStrip.Height / 2, rectTabStrip.Y + rectTabStrip.Height / 2));
            matrix.TransformPoints(pts);

            return new Rectangle((int)(pts[0].X - rect.Height / 2 + .5F),
                (int)(pts[0].Y - rect.Width / 2 + .5F),
                rect.Height, rect.Width);
        }
        Rectangle RtlTransform(Rectangle rect, DockStyle dockStyle)
        {
            if (IsVertical(dockStyle))
                return rect;
            else
                return this.RtlTransformRect(rect);
        }
        GraphicsPath GetTabOutline(StripTab tab, bool transformed, bool rtlTransform)
        {
            var dockStyle = tab.Content.Handler.DockStyle;
            var rectTab = GetTabRectangle(tab, transformed);
            if (rtlTransform)
                rectTab = RtlTransform(rectTab, dockStyle);
            bool upTab = (dockStyle == DockStyle.Left || dockStyle == DockStyle.Bottom);
            return rectTab.GetUpRoundCorner();
        }
        Rectangle GetTabRectangle(StripTab tab)
        {
            return GetTabRectangle(tab, false);
        }
        Rectangle GetTabRectangle(StripTab tab, bool transformed)
        {
            var dockStyle = tab.Content.Handler.DockStyle;
            var tabStripRect = GetLogicalTabStripRectangle(dockStyle);

            if (tabStripRect.IsEmpty)
                return Rectangle.Empty;

            var x = tab.Left;
            var y = tabStripRect.Y + (IsVertical(dockStyle) ? 0 : TabMargin.Top);
            var width = tab.DisplayWidth;
            var height = tabStripRect.Height - TabMargin.Top;

            var rect = new Rectangle(x, y, width, height);
            if (transformed)
                rect = GetTransformedRectangle(dockStyle, rect);
            return rect;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            LayoutTabs();
            base.OnLayout(levent);
        }
        void LayoutTabs()
        {
            LayoutTabs(DockStyle.Left);
            LayoutTabs(DockStyle.Top);
            LayoutTabs(DockStyle.Right);
            LayoutTabs(DockStyle.Bottom);
        }
        void LayoutTabs(DockStyle dockStyle)
        {
            var tabStripRect = GetLogicalTabStripRectangle(dockStyle);

            var imageHeight = tabStripRect.Height - ImgMargin.Vertical;
            var imageWidth = ImgSize.Width;
            if (imageHeight != ImgSize.Height)
                imageWidth = (ImgSize.Width * imageHeight) / ImgSize.Height;
            var x = TabMargin.Left + tabStripRect.Left;
            foreach (var grid in GetGrids(dockStyle))
            {
                foreach (var tab in grid.AutoHideTabs)
                {
                    int width = imageWidth + ImgMargin.Horizontal +
                        TextRenderer.MeasureText(tab.Content.Handler.TabText, TextFont).Width +
                        TextMargin.Horizontal;
                    tab.Left = x;
                    tab.DisplayWidth = width;
                    x += width;
                }
                x += TabGap;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(SystemBrushes.ControlDark, ClientRectangle);
            DrawTabStrip(g);
        }
        void DrawTabStrip(Graphics g)
        {
            DrawTabStrip(g, DockStyle.Left);
            DrawTabStrip(g, DockStyle.Top);
            DrawTabStrip(g, DockStyle.Right);
            DrawTabStrip(g, DockStyle.Bottom);
        }
        void DrawTabStrip(Graphics g, DockStyle dockStyle)
        {
            var tabStripRect = GetLogicalTabStripRectangle(dockStyle);

            if (tabStripRect.IsEmpty)
                return;

            var rawMatrix = g.Transform;

            if (IsVertical(dockStyle))
            {
                Matrix matrixRotated = new Matrix();
                matrixRotated.RotateAt(90, new PointF(tabStripRect.X + tabStripRect.Height / 2,
                    tabStripRect.Y + tabStripRect.Height / 2));
                g.Transform = matrixRotated;
            }

            foreach (var tab in GetGrids(dockStyle).SelectMany(grid => grid.AutoHideTabs))
            {
                DrawTab(g, tab);
            }
            g.Transform = rawMatrix;
        }

        void DrawTab(Graphics g, StripTab tab)
        {
            Rectangle tabOriginRect = GetTabRectangle(tab);
            if (tabOriginRect.IsEmpty)
                return;

            var dockStyle = tab.Content.Handler.DockStyle;
            IDockContent content = tab.Content;

            GraphicsPath path = GetTabOutline(tab, false, true);
            g.FillPath(SystemBrushes.ControlText, path);
            g.DrawPath(SystemPens.GrayText, path);

            // Set no rotate for drawing icon and text
            Matrix matrixRotate = g.Transform;

            g.ResetTransform();

            // Draw the icon
            Rectangle rectImage = tabOriginRect;
            rectImage.X += ImgMargin.Left;
            rectImage.Y += ImgMargin.Top;
            int imageHeight = tabOriginRect.Height - ImgMargin.Vertical;
            int imageWidth = ImgSize.Width;
            if (imageHeight > ImgSize.Height)
                imageWidth = ImgSize.Width * (imageHeight / ImgSize.Height);
            rectImage.Height = imageHeight;
            rectImage.Width = imageWidth;
            rectImage = GetTransformedRectangle(dockStyle, rectImage);

            if (IsVertical(dockStyle))
            {
                // The DockState is DockLeftAutoHide or DockRightAutoHide, so rotate the image 90 degrees to the right. 
                var rectTransform = RtlTransform(rectImage, dockStyle);
                Point[] rotationPoints =
                { 
                    new Point(rectTransform.X + rectTransform.Width, rectTransform.Y), 
                    new Point(rectTransform.X + rectTransform.Width, rectTransform.Y + rectTransform.Height), 
                    new Point(rectTransform.X, rectTransform.Y)
                };

                using (Icon rotatedIcon = new Icon(tab.Content.Handler.Icon, 16, 16))
                {
                    g.DrawImage(rotatedIcon.ToBitmap(), rotationPoints);
                }
            }
            else
            {
                g.DrawIcon(tab.Content.Handler.Icon, RtlTransform(rectImage, dockStyle));
            }

            // Draw the text
            Rectangle rectText = tabOriginRect;
            rectText.X += ImgMargin.Horizontal + imageWidth + TabGap;
            rectText.Width -= ImgMargin.Horizontal + imageWidth + TabGap;
            rectText = RtlTransform(GetTransformedRectangle(dockStyle, rectText), dockStyle);

            g.DrawString(content.Handler.TabText, TextFont, SystemBrushes.GradientActiveCaption, rectText, IsVertical(dockStyle) ? TabStringFormatVertical : TabStringFormatHorizontal);

            // Set rotate back
            g.Transform = matrixRotate;
        }

        protected override IDockContent HitTest(Point ptMouse)//探测当前鼠标位置的IDockContent，没有则返回null
        {
            var tab = new[] { DockStyle.Left, DockStyle.Top, DockStyle.Right, DockStyle.Bottom }
                .Where(s => GetLogicalTabStripRectangle(s, true).Contains(ptMouse))
                .SelectMany(s => GetGrids(s)).SelectMany(g => g.AutoHideTabs)
                .FirstOrDefault(t => GetTabOutline(t, true, true).IsVisible(ptMouse));
            return tab == null ? null : tab.Content;
        }

        protected override void OnRefreshChanges()
        {
            LayoutTabs();
            Invalidate();
        }

        protected internal override int StripHeight
        {
            get { return Math.Max(ImgMargin.Vertical + ImgSize.Height, TextMargin.Vertical + TextFont.Height) + TabGap; }
        }
    }
}
