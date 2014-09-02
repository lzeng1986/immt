using System.Drawing;

namespace LazyBones.Drawing
{
    public class Painter
    {
        public Rectangle Bound = Rectangle.Empty;
        public string Text = null;
        public PaintStyle Normal { get; set; }
        public PaintStyle Hover { get; set; }
        internal void Draw(Graphics g,Point pos)
        {
            if (Bound.Contains(pos))
            {
                Draw(g, Hover);
            } 
            else
            {
                Draw(g, Normal);
            }
        }
        void Draw(Graphics g, PaintStyle style)
        {
            if (style == null)
            {
                return;
            }
            using (var brush = new SolidBrush(style.BackColor))
            {
                g.FillRectangle(brush,Bound);
            }
            using (var pen = new Pen(style.BorderColor, style.BorderSize))
            {
                g.DrawRectangle(pen, Bound);
            }
        }
    }
}
