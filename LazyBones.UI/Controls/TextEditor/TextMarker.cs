using System.Drawing;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 文本标记
    /// </summary>
    public class TextMarker : MarkerBase
    {
        public TextMarkerType Type { get; private set; }

        public Color Color { get; private set; }

        public Color ForeColor { get; private set; }

        public bool OverrideForeColor { get; private set; }

        public bool ReadOnly { get; set; }

        public string ToolTip { get; private set; }

        public int EndOffset
        {
            get { return offset + length - 1; }
        }

        public int Offset { get { return offset; } }

        public int Length { get { return length; } }

        public TextMarker(int offset, int length, TextMarkerType textMarkerType)
            : this(offset, length, textMarkerType, Color.Red)
        {
        }

        public TextMarker(int offset, int length, TextMarkerType textMarkerType, Color color)
            : this(offset, length, textMarkerType, Color.Red, Color.Black)
        {
            this.OverrideForeColor = false;
        }

        public TextMarker(int offset, int length, TextMarkerType textMarkerType, Color color, Color foreColor)
        {
            if (length < 1) length = 1;
            this.offset = offset;
            this.length = length;
            this.Type = textMarkerType;
            this.Color = color;
            this.ForeColor = foreColor;
            this.OverrideForeColor = true;
        }
    }
    public enum TextMarkerType
    {
        Invisible,
        Underline,
        WaveLine
    }
}
