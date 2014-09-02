using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.Gantt
{
    interface IViewport
    {
        Matrix Projection { get; }
        RectangleF ViewRectangle { get; }
        void Resize();
        PointF DeviceToWorldCoord(PointF screencoord);
        float WorldHeight { get; set; }
        PointF WorldToDeviceCoord(PointF worldcoord);
        float WorldWidth { get; set; }
        float X { get; set; }
        float Y { get; set; }
    }

    /// <summary>
    /// IViewport for printing to file
    /// </summary>
    class PrintViewport : IViewport
    {
        public PrintViewport(Graphics graphics,
            float worldWidth, float worldHeight,
            int deviceWidth, int deviceHeight,
            int marginLeft, int marginTop)
        {
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;

            _mDeviceWidth = deviceWidth;
            _mDeviceHeight = deviceHeight;

            _mMarginTop = marginTop;
            _mMarginLeft = marginLeft;
        }

        /// <summary>
        /// Get or set viewport X world coordinate
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Get or set viewport Y world coordinate
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Get or set scaling factor between 0.0f and 1.0f (obviously, scale of zero would mean you can't see anything)
        /// </summary>
        public float Scale
        {
            get { return _mScale; }
            set { _mScale = value; }
        }

        /// <summary>
        /// Get or set width of the world
        /// </summary>
        public float WorldWidth { get; set; }

        /// <summary>
        /// Get or set height of the world
        /// </summary>
        public float WorldHeight { get; set; }

        /// <summary>
        /// Get the projection matrix for transforming models into viewport
        /// </summary>
        public Matrix Projection
        {
            get
            {
                Matrix matrix = new Matrix();
                matrix.Translate(this._mMarginLeft, this._mMarginTop);
                matrix.Scale(_mScale, _mScale);
                matrix.Translate(-this.X, -this.Y);
                return matrix;
            }
        }

        /// <summary>
        /// Get the rectangle bounds of the viewport in world coordinate
        /// </summary>
        public RectangleF ViewRectangle
        {
            get { return new RectangleF(this.X, this.Y, _mDeviceWidth / _mScale, _mDeviceHeight / _mScale); }
        }
        public RectangleF WholeRectangle { get; private set; }
        /// <summary>
        /// Resize the viewport, recalculating and correcting dimensions
        /// </summary>
        public void Resize()
        {


        }

        /// <summary>
        /// Convert view coordinates to world coordinate
        /// </summary>
        /// <param name="screencoord"></param>
        /// <returns></returns>
        public PointF DeviceToWorldCoord(Point screencoord)
        {
            return new PointF(screencoord.X + X, screencoord.Y + Y);
        }

        /// <summary>
        /// Convert view coordinates to world coordinate
        /// </summary>
        /// <param name="screencoord"></param>
        /// <returns></returns>
        public PointF DeviceToWorldCoord(PointF screencoord)
        {
            return new PointF(screencoord.X + X, screencoord.Y + Y);
        }

        /// <summary>
        /// Convert world coordinates to view coordinate
        /// </summary>
        /// <param name="worldcoord"></param>
        /// <returns></returns>
        public PointF WorldToDeviceCoord(PointF worldcoord)
        {
            return new PointF(worldcoord.X - X, worldcoord.Y - Y);
        }

        int _mDeviceWidth, _mDeviceHeight;
        int _mMarginLeft, _mMarginTop;

        float _mScale = 1.0f;
    }

    /// <summary>
    /// An IViewport that is placed over a world coordinate system and provides methods to transform between world and view coordinates
    /// </summary>
    class ControlViewport : IViewport
    {
        Control device;
        RectangleF viewRectangle = RectangleF.Empty;
        Matrix matrix = new Matrix();
        float worldHeight, worldWidth;

        HScrollBar hScrollBar = new HScrollBar();
        VScrollBar vScrollBar = new VScrollBar();
        Control hole = new Control();

        public ControlViewport(Control view)
        {
            device = view;
            hScrollBar.Left = 0;
            vScrollBar.Top = 0;
            device.Controls.AddRange(new Control[] { hScrollBar, vScrollBar, hole });
            WorldWidth = view.Width;
            WorldHeight = view.Height;
            hScrollBar.Scroll += OnScroll;
            vScrollBar.Scroll += OnScroll;
            device.Resize += (s, e) => Resize();
            device.MouseWheel += device_MouseWheel;
            RecalculateMatrix();
            RecalculateRectangle();
        }

        void device_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Control))
                return;
            if (e.Delta > 0)
            {
                vScrollBar.Value = Math.Max(vScrollBar.Minimum,vScrollBar.Value - vScrollBar.SmallChange);
            }
            else
            {
                vScrollBar.Value = Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange, vScrollBar.Value + vScrollBar.SmallChange);
            }
            RecalculateMatrix();
            RecalculateRectangle();
            device.Invalidate();
        }

        void OnScroll(object sender, ScrollEventArgs e)
        {
            CalcScroll();
            RecalculateRectangle();
            RecalculateMatrix();
            device.Invalidate();
        }

        public static readonly Matrix Identity = new Matrix(1, 0, 0, 1, 0, 0);

        public int WheelDelta { get; set; }

        public RectangleF ViewRectangle
        {
            get { return viewRectangle; }
        }
        public Matrix Projection
        {
            get { return matrix; }
        }
        Size InnerClientSize
        {
            get
            {
                var heigth = hScrollBar.Visible ? device.Height - hScrollBar.Height : device.Height;
                var width = vScrollBar.Visible ? device.Width - vScrollBar.Width : device.Width;
                return new Size(Math.Max(width, 0), Math.Max(heigth, 0));
            }
        }
        Rectangle InnerClientRect
        {
            get { return new Rectangle(Point.Empty, InnerClientSize); }
        }
        public void Resize()
        {
            CalcScroll();
            RecalculateRectangle();
            RecalculateMatrix();
        }
        void CalcScroll()
        {
            if (WorldWidth > device.Width && !hScrollBar.Visible)
            {
                hScrollBar.Show();
                hScrollBar.Value = 0;
            }
            else if (WorldWidth <= device.Width && hScrollBar.Visible)
            {
                hScrollBar.Hide();
                hScrollBar.Value = 0; 
            }
            if (WorldHeight > device.Height && !vScrollBar.Visible)
            {
                vScrollBar.Show();
                vScrollBar.Value = 0;
            }
            else if (WorldHeight <= device.Height && vScrollBar.Visible)
            {
                vScrollBar.Hide();
                vScrollBar.Value = 0;
            }

            var rectClient = InnerClientRect;

            if (vScrollBar.Visible == true)
            {
                vScrollBar.Left = rectClient.Right;
                vScrollBar.Height = rectClient.Height;
                vScrollBar.LargeChange = rectClient.Height;
                vScrollBar.Maximum = (int)WorldHeight;
                vScrollBar.SmallChange = vScrollBar.LargeChange / 5;
                vScrollBar.Value = Math.Min(vScrollBar.Value, vScrollBar.Maximum - vScrollBar.LargeChange);
            }

            if (hScrollBar.Visible == true)
            {
                hScrollBar.Top = rectClient.Bottom;
                hScrollBar.Width = rectClient.Width;
                hScrollBar.LargeChange = rectClient.Width;
                hScrollBar.Maximum = (int)WorldWidth;
                hScrollBar.SmallChange = hScrollBar.LargeChange / 5;
                hScrollBar.Value = Math.Min(hScrollBar.Value, hScrollBar.Maximum - hScrollBar.LargeChange);
            }

            hole.Visible = vScrollBar.Visible && hScrollBar.Visible;
            if (hole.Visible)
            {
                hole.Bounds = new Rectangle(vScrollBar.Left, hScrollBar.Top, vScrollBar.Width, hScrollBar.Height);
            }
        }

        public PointF DeviceToWorldCoord(PointF screencoord)
        {
            return new PointF(screencoord.X + hScrollBar.Value, screencoord.Y + vScrollBar.Value);
        }

        public PointF WorldToDeviceCoord(PointF worldcoord)
        {
            return new PointF(worldcoord.X - hScrollBar.Value, worldcoord.Y - vScrollBar.Value);
        }

        public float WorldWidth
        {
            get { return worldWidth; }
            set
            {
                if (value != worldWidth)
                {
                    worldWidth = value;
                    CalcScroll();
                    RecalculateRectangle();
                    RecalculateMatrix();
                }
            }
        }

        public float WorldHeight
        {
            get { return worldHeight; }
            set
            {
                if (value != worldHeight)
                {
                    worldHeight = value;
                    CalcScroll();
                    RecalculateRectangle();
                    RecalculateMatrix();
                }
            }
        }

        public float X
        {
            get { return hScrollBar.Value; }
            set
            {
                if (value != hScrollBar.Value && hScrollBar.Visible)
                {
                    value = Math.Min(value, hScrollBar.Maximum - hScrollBar.LargeChange);
                    value = Math.Max(value, 0);
                    hScrollBar.Value = (int)value;
                    RecalculateRectangle();
                    RecalculateMatrix();
                    device.Invalidate();
                }
            }
        }

        public float Y
        {
            get { return vScrollBar.Value; }
            set
            {
                if (value != vScrollBar.Value && vScrollBar.Visible)
                {
                    value = Math.Min(value, vScrollBar.Maximum - vScrollBar.LargeChange);
                    value = Math.Max(value, 0);
                    vScrollBar.Value = (int)value;
                    RecalculateRectangle();
                    RecalculateMatrix();
                    //device.Invalidate();
                }
            }
        }

        void RecalculateRectangle()
        {
            viewRectangle = new RectangleF(new PointF(X, Y), InnerClientSize);
        }

        private void RecalculateMatrix()
        {
            matrix = new Matrix();
            matrix.Translate(-X, -Y);
        }
    }
}
