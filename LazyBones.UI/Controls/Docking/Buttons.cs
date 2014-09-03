using System;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    class SimpleButton : ButtonBase
    {
        public SimpleButton(Bitmap bmp)
        {
            BackgroundImage = bmp;
            BackgroundImageLayout = ImageLayout.Stretch;
        }
        bool isMouseOver;
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isMouseOver = true;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isMouseOver = false;
        }
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (isMouseOver && Enabled)
                ControlPaint.DrawBorder(pevent.Graphics, Rectangle.Inflate(ClientRectangle, -1, -1), ForeColor, ButtonBorderStyle.Solid);
        }
    }
    class ListButton : SimpleButton
    {
        Bitmap m_image0, m_image1;

        public ListButton(Bitmap image0, Bitmap image1)
            : base(image0)
        {
            m_image0 = image0;
            m_image1 = image1;
        }
        bool overflowed = true;
        public bool Overflowed
        {
            get { return overflowed; }
            set
            {
                if (overflowed == value)
                    return;
                overflowed = value;
                BackgroundImage = overflowed ? m_image0 : m_image1;
            }
        }
    }
    class AutoHideButton : SimpleButton
    {
        Bitmap dock, autoHide;
        DockGridCaption caption;
        public AutoHideButton(DockGridCaption caption, Bitmap dockBmp, Bitmap autoHideBmp)
            : base(dockBmp)
        {
            dock = dockBmp;
            autoHide = autoHideBmp;
            this.caption = caption;
        }
        public override Image BackgroundImage
        {
            get { return caption.IsAutoHide ? autoHide : dock; }
            set { base.BackgroundImage = value; }
        }
    }
}
