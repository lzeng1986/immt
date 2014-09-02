using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 处理拖拽时的轮廓
    /// </summary>
    class DockOutline
    {
        FormDrag dragForm;
        Rectangle oldFloatWindowBounds;
        Control oldDockTo;
        DockStyle oldDockStyle;
        int oldContentIndex;
        public DockOutline()
        {
            ResetValues();
            SaveOldValues();
            dragForm = new FormDrag();
            SetDragForm(Rectangle.Empty);
            dragForm.BackColor = SystemColors.ActiveCaption;
            dragForm.Opacity = 0.5;
            dragForm.Show();
        }

        public Rectangle FloatWindowBounds { get; private set; }
        public Control DockTo { get; private set; }
        public DockStyle DockStyle { get; private set; }
        public int ContentIndex { get; private set; }
        public bool FlagTestDrop { get; set; }
        bool SameAsOldValue
        {
            get
            {
                return FloatWindowBounds == oldFloatWindowBounds &&
                    DockTo == oldDockTo &&
                    DockStyle == oldDockStyle &&
                    ContentIndex == oldContentIndex;
            }
        }
        public bool FlagFullEdge
        {
            get { return ContentIndex != 0; }
        }

        void SaveOldValues()
        {
            oldDockTo = DockTo;
            oldDockStyle = DockStyle;
            oldContentIndex = ContentIndex;
            oldFloatWindowBounds = FloatWindowBounds;
        }
        void ResetValues()
        {
            FloatWindowBounds = Rectangle.Empty;
            DockTo = null;
            DockStyle = DockStyle.None;
            ContentIndex = -1;
            FlagTestDrop = true;
        }
        public void Show()
        {
            SaveOldValues();
            ResetValues();
            TryShow();
        }

        public void Show(DockGrid grid, DockStyle dockStyle)
        {
            SaveOldValues();
            ResetValues();
            DockTo = grid;
            DockStyle = dockStyle;
            TryShow();
        }

        public void Show(DockGrid grid, int contentIndex)
        {
            SaveOldValues();
            ResetValues();
            DockTo = grid;
            DockStyle = DockStyle.Fill;
            ContentIndex = contentIndex;
            TryShow();
        }

        public void Show(DockPanel dockPanel, DockStyle dockStyle, bool fullPanelEdge)
        {
            SaveOldValues();
            ResetValues();
            DockTo = dockPanel;
            DockStyle = dockStyle;
            ContentIndex = fullPanelEdge ? -1 : 0;
            TryShow();
        }

        public void Show(Rectangle floatWindowBounds)
        {
            SaveOldValues();
            ResetValues();
            FloatWindowBounds = floatWindowBounds;
            TryShow();
        }
        void TryShow()
        {
            if (!SameAsOldValue)
                CalculateRegion();
        }
        public void Close()
        {
            dragForm.Close();
        }
        void CalculateRegion()
        {
            if (SameAsOldValue)
                return;
            if (!FloatWindowBounds.IsEmpty)
                SetOutline(FloatWindowBounds);
            else if (DockTo is DockPanel)
                SetOutline(DockTo as DockPanel, DockStyle);
            else if (DockTo is DockGrid)
                SetOutline(DockTo as DockGrid, DockStyle, ContentIndex);
            else
                SetOutline();
        }
        void SetOutline()
        {
            SetDragForm(Rectangle.Empty);
        }
        void SetOutline(Rectangle floatWindowBounds)
        {
            SetDragForm(floatWindowBounds);
        }
        void SetOutline(DockPanel dockPanel, DockStyle dockStyle)
        {
            var rect = dockPanel.RectangleToScreen(dockPanel.DockBounds);

            var size = dockPanel.DockWindows.GetDockWindowSize(dockStyle);
            if (dockStyle == DockStyle.Top)
            {
                rect.Height = size;
            }
            else if (dockStyle == DockStyle.Bottom)
            {
                rect.Y = -size;
                rect.Height = size;
            }
            else if (dockStyle == DockStyle.Left)
            {
                rect.Width = size;
            }
            else if (dockStyle == DockStyle.Right)
            {
                rect.X = -size;
                rect.Width = size;
            }
            else if (dockStyle == DockStyle.Fill)
            {
                rect = dockPanel.DocumentDockBounds;
                rect = dockPanel.RectangleToScreen(rect);
            }

            SetDragForm(rect);
        }
        void SetOutline(DockGrid grid, DockStyle dockStyle, int contentIndex)
        {
            if (dockStyle != DockStyle.Fill)
            {
                Rectangle rect = grid.DisplayRectangle;
                if (dockStyle == DockStyle.Right)
                {
                    rect.X += rect.Width / 2;
                    rect.Width /= 2;
                }
                if (dockStyle == DockStyle.Bottom)
                {
                    rect.Y += rect.Height / 2;
                    rect.Height /= 2;
                }
                if (dockStyle == DockStyle.Left)
                    rect.Width /= 2;
                if (dockStyle == DockStyle.Top)
                    rect.Height /= 2;
                rect = grid.RectangleToScreen(rect);
                SetDragForm(rect);
            }
            else if (contentIndex == -1)
            {
                var rect = grid.DisplayRectangle;
                rect = grid.RectangleToScreen(rect);
                SetDragForm(rect);
            }
            else
            {
                using (GraphicsPath path = grid.TabStrip.GetOutline(contentIndex))
                {
                    RectangleF rectF = path.GetBounds();
                    Rectangle rect = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
                    using (Matrix matrix = new Matrix(rect, new Point[] { new Point(0, 0), new Point(rect.Width, 0), new Point(0, rect.Height) }))
                    {
                        path.Transform(matrix);
                    }
                    Region region = new Region(path);
                    SetDragForm(rect, region);
                }
            }
        }
        void SetDragForm(Rectangle bounds)
        {
            SetDragForm(bounds, bounds.IsEmpty ? new Region(bounds) : null);
        }
        void SetDragForm(Rectangle bounds, Region region)
        {
            dragForm.Bounds = bounds;
            dragForm.Region = region;
        }
    }
}
