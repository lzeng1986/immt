using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 提供Brush对象和Pen对象的缓存
    /// </summary>
    static class BrushPool
    {
        static Dictionary<int, Brush> brushes = new Dictionary<int, Brush>();
        static Dictionary<int, Pen> pens = new Dictionary<int, Pen>();

        internal static Brush GetBrush(Color color)
        {
            var key = color.ToArgb();
            if (brushes.ContainsKey(key))
                return brushes[key];
            var brush = new SolidBrush(color);
            brushes.Add(key, brush);
            return brush;
        }        
        internal static Pen GetPen(Color color)
        {
            var key = color.ToArgb();
            if (pens.ContainsKey(key))
                return pens[key];
            var pen = new Pen(color);
            pens.Add(key, pen);
            return pen;
        }
        internal static Pen GetPen(Color bgColor, Color fgColor)
        {
            var key = 87 ^ bgColor.ToArgb() ^ fgColor.ToArgb();
            if (pens.ContainsKey(key))
                return pens[key];
            var brush = new HatchBrush(HatchStyle.Percent50, bgColor, fgColor);
            Pen pen = new Pen(brush);
            pens.Add(key, pen);
            return pen;
        }
    }
}
