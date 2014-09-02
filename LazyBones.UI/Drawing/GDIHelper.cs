using System;
using System.Drawing;

namespace LazyBones.Drawing
{
    public static class GDIHelper
    {
        public static Font GetAppropriateFont(Graphics g, Size layoutSize, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return (Font)font.Clone();
            var bound = g.MeasureString(text, font);
            var hRate = layoutSize.Height / bound.Height;
            var wRate = layoutSize.Width / bound.Width;
            var newSize = font.Size * Math.Min(hRate, wRate);
            return new Font(font.FontFamily, newSize);
        }
        public static void DrawStringInRect(Graphics g, Rectangle bound, string text, Font font, Brush brush)
        {
            if (string.IsNullOrEmpty(text))
                return;
            using (var drawFont = GetAppropriateFont(g, bound.Size, text, font))
            {
                g.DrawString(text, drawFont, brush, bound);
            }
        }

    }
}
