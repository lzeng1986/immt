using System;
using System.Drawing;
using System.Globalization;
using System.Xml;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.TextEditor
{
    //文本绘制信息，包括文本的颜色，字体信息，主要用于高亮显示
    public class TextDrawInfo
    {
        static Color DefaultBackColor = Color.WhiteSmoke;
        static Color DefaultForeColor = Color.Black;
        public static readonly TextDrawInfo Default = new TextDrawInfo(DefaultForeColor);

        public Color BackColor { get; private set; }
        public Color ForeColor { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }

        public TextDrawInfo()
            : this(DefaultForeColor)
        {
        }
        public TextDrawInfo(Color foreColor)
            : this(foreColor, DefaultBackColor)
        { }
        public TextDrawInfo(Color foreColor, Color backColor)
            : this(foreColor, backColor, false, false)
        {
        }
        public TextDrawInfo(Color foreColor, bool bold, bool italic)
            : this(foreColor, DefaultBackColor, bold, italic)
        {
        }
        public TextDrawInfo(Color foreColor, Color backColor, bool bold, bool italic)
        {
            BackColor = backColor;
            ForeColor = foreColor;
            Bold = bold;
            Italic = italic;
        }
        public TextDrawInfo(XmlElement el)
        {
            ParamGuard.NotNull(el, "el");
            ForeColor = Color.Black;
            if (el.Attributes["fgcolor"] != null)
            {
                string c = el.Attributes["fgcolor"].InnerText;
                if (c[0] == '#')
                {
                    ForeColor = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    var name = c.Substring("SystemColors.".Length);
                    if (Enum.IsDefined(typeof(KnownColor), name))
                    {
                        ForeColor = Color.FromKnownColor((KnownColor)Enum.Parse(typeof(KnownColor), name));
                    }
                }
            }
            BackColor = DefaultBackColor;
            if (el.Attributes["bgcolor"] != null)
            {
                string c = el.Attributes["bgcolor"].InnerText;
                if (c[0] == '#')
                {
                    BackColor = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    var name = c.Substring("SystemColors.".Length);
                    if (Enum.IsDefined(typeof(KnownColor), name))
                    {
                        BackColor = Color.FromKnownColor((KnownColor)Enum.Parse(typeof(KnownColor), name));
                    }
                }
            }
        }
        static Color ParseColor(string c)
        {
            var i = int.Parse(c.Substring(1), NumberStyles.HexNumber);
            var a = i >> 24;
            var r = (i >> 16) & 0xFF;
            var g = (i >> 8) & 0xFF;
            var b = i & 0xFF;
            return Color.FromArgb(a, r, g, b);
        }
    }
}
