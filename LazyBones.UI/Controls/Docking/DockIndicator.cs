using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    class DockIndicator : FormDrag
    {
        static readonly Padding PanelIndicatorMargin = new Padding(10);

        GridIndicator gridIndicator = new GridIndicator();
        PanelIndicator panelLeft = new PanelIndicator(DockStyle.Left);
        PanelIndicator panelTop = new PanelIndicator(DockStyle.Top);
        PanelIndicator panelRight = new PanelIndicator(DockStyle.Right);
        PanelIndicator panelBottom = new PanelIndicator(DockStyle.Bottom);
        PanelIndicator panelFill = new PanelIndicator(DockStyle.Fill);
        IHitTest[] hitTestList;
        public DockIndicator(DockDragHandler dragHandler)
        {
            Handler = dragHandler;
            DockPanel = dragHandler.DockPanel;
            hitTestList = new IHitTest[] { gridIndicator, panelLeft, panelTop, panelRight, panelBottom, panelFill };
            Controls.AddRange(hitTestList.Cast<Control>().ToArray());
            Region = new Region(Rectangle.Empty);
        }

        private bool fullPanelEdge = false;
        public bool FullPanelEdge
        {
            get { return fullPanelEdge; }
            set
            {
                if (fullPanelEdge == value)
                    return;
                fullPanelEdge = value;
                RefreshChanges();
            }
        }
        public DockDragHandler Handler { get; private set; }
        public DockPanel DockPanel { get; private set; }

        DockGrid dockGrid = null;
        public DockGrid DockGrid
        {
            get { return dockGrid; }
            internal set
            {
                if (dockGrid == value)
                    return;
                var oldDisplayingPane = DisplayingGrid;
                dockGrid = value;
                if (oldDisplayingPane != DisplayingGrid)
                    RefreshChanges();
            }
        }

        IHitTest hitTest = null;
        IHitTest HitTestResult
        {
            get { return hitTest; }
            set
            {
                if (hitTest == value)
                    return;
                if (hitTest != null)
                    hitTest.Status = DockStyle.None;
                hitTest = value;
            }
        }

        DockGrid DisplayingGrid
        {
            get { return GridDiamondVisible ? dockGrid : null; }
        }
        bool GridDiamondVisible
        {
            get
            {
                if (dockGrid == null)
                    return false;
                if (!DockPanel.AllowEndUserNestedDocking)
                    return false;
                return Handler.DragSource.CanDockTo(dockGrid);
            }
        }
        void RefreshChanges()
        {
            var region = new Region(Rectangle.Empty);
            var dockArea = FullPanelEdge ? DockPanel.DockBounds : DockPanel.DocumentDockBounds;

            dockArea = RectangleToClient(DockPanel.RectangleToScreen(dockArea));    //调整显示边界居中
            if (ShouldPanelIndicatorVisible(DockStyle.Left))
            {
                panelLeft.Left = dockArea.Left + PanelIndicatorMargin.Left;
                panelLeft.Top = dockArea.Top + (dockArea.Height - panelLeft.Height) / 2;
                panelLeft.Visible = true;
                region.Union(panelLeft.Bounds);
            }
            else
                panelLeft.Visible = false;

            if (ShouldPanelIndicatorVisible(DockStyle.Right))
            {
                panelRight.Left = dockArea.Right - panelRight.Width - PanelIndicatorMargin.Right;
                panelRight.Top = dockArea.Top + (dockArea.Height - panelRight.Height) / 2;
                panelRight.Visible = true;
                region.Union(panelRight.Bounds);
            }
            else
                panelRight.Visible = false;

            if (ShouldPanelIndicatorVisible(DockStyle.Top))
            {
                panelTop.Left = dockArea.Left + (dockArea.Width - panelTop.Width) / 2;
                panelTop.Top = dockArea.Top + PanelIndicatorMargin.Top;
                panelTop.Visible = true;
                region.Union(panelTop.Bounds);
            }
            else
                panelTop.Visible = false;

            if (ShouldPanelIndicatorVisible(DockStyle.Bottom))
            {
                panelBottom.Left = dockArea.Left + (dockArea.Width - panelBottom.Width) / 2;
                panelBottom.Top = dockArea.Bottom - panelBottom.Height - PanelIndicatorMargin.Bottom;
                panelBottom.Visible = true;
                region.Union(panelBottom.Bounds);
            }
            else
                panelBottom.Visible = false;

            if (ShouldPanelIndicatorVisible(DockStyle.Fill))
            {
                var documentWindowBounds = RectangleToClient(DockPanel.RectangleToScreen(DockPanel.DocumentDockBounds));
                panelFill.Left = documentWindowBounds.Left + (documentWindowBounds.Width - panelFill.Width) / 2;
                panelFill.Top = documentWindowBounds.Top + (documentWindowBounds.Height - panelFill.Height) / 2;
                panelFill.Visible = true;
                region.Union(panelFill.Bounds);
            }
            else
                panelFill.Visible = false;

            if (GridDiamondVisible)
            {
                Rectangle rect = RectangleToClient(DockGrid.RectangleToScreen(DockGrid.ClientRectangle));
                gridIndicator.Left = rect.Left + (rect.Width - gridIndicator.Width) / 2;
                gridIndicator.Top = rect.Top + (rect.Height - gridIndicator.Height) / 2;
                gridIndicator.Visible = true;
                using (var graphicsPath = GridIndicator.DisplayingGraphicsPath.Clone() as GraphicsPath)
                {
                    Point[] pts = new Point[]
                        {
                            new Point(gridIndicator.Left, gridIndicator.Top),
                            new Point(gridIndicator.Right,gridIndicator.Top),
                            new Point(gridIndicator.Left, gridIndicator.Bottom)
                        };
                    using (var matrix = new Matrix(gridIndicator.ClientRectangle, pts))
                    {
                        graphicsPath.Transform(matrix);
                    }
                    region.Union(graphicsPath);
                }
            }
            else
                gridIndicator.Visible = false;

            Region = region;
        }

        bool ShouldPanelIndicatorVisible(DockStyle dockStyle)
        {
            if (!Visible)
                return false;

            if (DockPanel.DockWindows[dockStyle].Visible)
                return false;

            return Handler.DragSource.IsDockValid(dockStyle);
        }
        public override void ShowAndActivate()
        {
            base.ShowAndActivate();
            Bounds = SystemInformation.VirtualScreen;
            RefreshChanges();
        }

        public void TestDrop()
        {
            DockGrid = DockPanel.GetGridAtCursor();

            var pos = Cursor.Position;
            HitTestResult = hitTestList.FirstOrDefault(h => TestDrop(h, pos) != DockStyle.None);
            if (HitTestResult != null)
            {
                if (HitTestResult is GridIndicator)
                    Handler.Outline.Show(DockGrid, HitTestResult.Status);
                else
                    Handler.Outline.Show(DockPanel, HitTestResult.Status, FullPanelEdge);
            }
        }

        static DockStyle TestDrop(IHitTest hitTest, Point screemPoint)
        {
            return hitTest.Status = hitTest.HitTest(screemPoint);
        }
    }
    interface IHitTest
    {
        DockStyle Status { get; set; }
        DockStyle HitTest(Point screemPoint);
    }
    class PanelIndicator : PictureBox, IHitTest
    {
        static Bitmap imgLeft = ControlRes.PanelLeft;
        static Bitmap imgRight = ControlRes.PanelRight;
        static Bitmap imgTop = ControlRes.PanelTop;
        static Bitmap imgBottom = ControlRes.PanelBottom;
        static Bitmap imgFill = ControlRes.PanelFill;
        static Bitmap imgLeftActive = ControlRes.PanelLeftActive;
        static Bitmap imgRightActive = ControlRes.PanelRightActive;
        static Bitmap imgTopActive = ControlRes.PanelTopActive;
        static Bitmap imgBottomActive = ControlRes.PanelBottomActive;
        static Bitmap imgFillActive = ControlRes.PanelFillActive;

        static Dictionary<DockStyle, Image> activeImgs = new Dictionary<DockStyle, Image>
        {
            {DockStyle.Left,imgLeftActive},{DockStyle.Right,imgRightActive},{DockStyle.Top,imgTopActive},
            {DockStyle.Right,imgRightActive},{DockStyle.Fill,imgFillActive},{DockStyle.None,null}
        };
        static Dictionary<DockStyle, Image> inactiveImgs = new Dictionary<DockStyle, Image>
        {
            {DockStyle.Left,imgLeft},{DockStyle.Right,imgRight},{DockStyle.Top,imgTop},
            {DockStyle.Right,imgRight},{DockStyle.Fill,imgFill},{DockStyle.None,null}
        };
        public PanelIndicator(DockStyle dockStyle)
        {
            this.dockStyle = dockStyle;
            SizeMode = PictureBoxSizeMode.AutoSize;
            Image = inactiveImgs[dockStyle];
        }
        DockStyle dockStyle;
        DockStyle status;
        public DockStyle Status
        {
            get { return status; }
            set
            {
                if (value != dockStyle && value != DockStyle.None)
                    throw new InvalidEnumArgumentException();

                if (status == value)
                    return;
                status = value;
                Image = (status != DockStyle.None) ? activeImgs[dockStyle] : inactiveImgs[dockStyle];
            }
        }
        public DockStyle HitTest(Point screemPoint)
        {
            return this.Visible && ClientRectangle.Contains(PointToClient(screemPoint)) ? dockStyle : DockStyle.None;
        }
    }
    class GridIndicator : PictureBox, IHitTest
    {
        static Bitmap imgLeft = ControlRes.GridLeft;
        static Bitmap imgRight = ControlRes.GridRight;
        static Bitmap imgTop = ControlRes.GridTop;
        static Bitmap imgBottom = ControlRes.GridBottom;
        static Bitmap imgFill = ControlRes.GridFill;
        static Bitmap imgGrid = ControlRes.Grid;
        static Bitmap imgHotspot = ControlRes.GridHotspot;
        static Bitmap imgHotspotIndex = ControlRes.GridHotspotIndex;

        static Dictionary<DockStyle, Image> imgs = new Dictionary<DockStyle, Image>
        {
            {DockStyle.Left,imgLeft},{DockStyle.Right,imgRight},{DockStyle.Top,imgTop},
            {DockStyle.Right,imgRight},{DockStyle.Fill,imgFill},{DockStyle.None,imgGrid}
        };

        static Dictionary<Point, DockStyle> hotSpots = new Dictionary<Point, DockStyle>
        {
            {new Point(1,0),DockStyle.Top},{new Point(0,1),DockStyle.Left},{new Point(1,1),DockStyle.Fill},
            {new Point(2,1),DockStyle.Right},{new Point(1,2),DockStyle.Bottom}
        };

        public static readonly GraphicsPath DisplayingGraphicsPath = imgGrid.CalculateGraphicsPath();

        public GridIndicator()
        {
            SizeMode = PictureBoxSizeMode.AutoSize;
            Image = imgGrid;
        }
        public DockStyle HitTest(Point pt)
        {
            if (!Visible)
                return DockStyle.None;
            pt = PointToClient(pt);
            if (!ClientRectangle.Contains(pt))
                return DockStyle.None;
            return hotSpots.FirstOrDefault(s => imgHotspot.GetPixel(pt.X, pt.Y) == imgHotspotIndex.GetPixel(s.Key.X, s.Key.Y)).Value;
        }

        DockStyle status = DockStyle.None;
        public DockStyle Status
        {
            get { return status; }
            set
            {
                status = value;
                Image = imgs[status];
            }
        }
    }
}
