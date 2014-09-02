using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace LazyBones.UI.Controls.Docking
{
    public abstract class ButtonBase : Control
    {
        protected ButtonBase()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public abstract Bitmap Bmp { get; }

        bool isMouseOver = false;
        protected bool IsMouseOver
        {
            get { return isMouseOver; }
            set
            {
                if (isMouseOver == value)
                    return;
                isMouseOver = value;
                Invalidate();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            IsMouseOver = ClientRectangle.Contains(e.Location);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            IsMouseOver = true;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            IsMouseOver = false;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (IsMouseOver && Enabled)
            {
                using (var pen = new Pen(ForeColor))
                {
                    e.Graphics.DrawRectangle(pen, Rectangle.Inflate(ClientRectangle, -1, -1));
                }
            }
            using (var imageAttributes = new ImageAttributes())
            {
                var colorMap = new[] 
                {
                     new ColorMap{OldColor = Color.FromArgb(0, 0, 0),NewColor = ForeColor},
                     new ColorMap{OldColor = Bmp.GetPixel(0, 0),NewColor = Color.Transparent}
                };

                imageAttributes.SetRemapTable(colorMap);

                e.Graphics.DrawImage(
                   Bmp,
                   new Rectangle(Point.Empty, Bmp.Size),
                   1, 1, Width - 1, Height - 1,
                   GraphicsUnit.Pixel,
                   imageAttributes);
            }
        }
        public void RefreshChanges()
        {
            if (IsDisposed)
                return;
            IsMouseOver = ClientRectangle.Contains(PointToClient(Cursor.Position));
            OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }
    }
    internal class SimpleButton : ButtonBase
    {
        public SimpleButton(Bitmap bmp)
        {
            this.bmp = bmp;
        }
        Bitmap bmp;
        public override Bitmap Bmp { get { return bmp; } }
    }

}
