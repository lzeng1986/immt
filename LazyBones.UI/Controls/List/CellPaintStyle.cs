using System.Drawing;

namespace LazyBones.UI.Controls.List
{
    public class CellPaintStyle
    {
        public Font Font { get; set; }
        public Brush BackBrush { get; set; }
        public Brush ForeBrush { get; set; }
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public ItemCell Cell { get; set; }
    }
}
