using System;
using System.Drawing;

namespace LazyBones.Extensions
{
    public static class ExtensionForGraphics
    {
        public static Font GetAppropriateFont(this Graphics g, Size layoutSize, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return (Font)font.Clone();
            var bound = g.MeasureString(text, font);
            var hRate = layoutSize.Height / bound.Height;
            var wRate = layoutSize.Width / bound.Width;
            var newSize = font.Size * Math.Min(hRate, wRate);
            return new Font(font.FontFamily, newSize);
        }
        public static void DrawStringInRect(this Graphics g, Rectangle bound, string text, Font font, Brush brush)
        {
            if (string.IsNullOrEmpty(text))
                return;
            using (var drawFont = GetAppropriateFont(g, bound.Size, text, font))
            {
                g.DrawString(text, drawFont, brush, bound);
            }
        }
        public static void DrawRectangle(this Graphics graphics, Pen pen, RectangleF rectangle)
        {
            graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }
}
