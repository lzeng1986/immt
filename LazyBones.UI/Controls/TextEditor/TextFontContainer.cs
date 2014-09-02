using System;
using System.Drawing;

namespace LazyBones.UI.Controls.TextEditor
{
    // 提供Font类的缓存
    public class TextFontContainer : IDisposable
    {
        public TextFontContainer(Font regularFont)
        {
            RegularFont = new Font(regularFont, FontStyle.Regular);
            BoldFont = new Font(RegularFont, FontStyle.Bold);
            ItalicFont = new Font(RegularFont, FontStyle.Italic);
            BoldItalicFont = new Font(RegularFont, FontStyle.Bold | FontStyle.Italic);
        }
        public Font RegularFont { get; private set; }
        public Font BoldFont { get; private set; }
        public Font ItalicFont { get; private set; }
        public Font BoldItalicFont { get; private set; }

        static float twipsPerPixelY = -1f;

        public static float TwipsPerPixelY
        {
            get
            {
                if (twipsPerPixelY < 0)
                {
                    using (Bitmap bmp = new Bitmap(1, 1))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            twipsPerPixelY = 1440 / g.DpiY;
                        }
                    }
                }
                return twipsPerPixelY;
            }
        }
        public void Dispose()
        {
            RegularFont.Dispose();
            BoldFont.Dispose();
            ItalicFont.Dispose();
            BoldItalicFont.Dispose();
            GC.SuppressFinalize(this);
        }
        public Font this[TextDrawInfo color]
        {
            get
            {
                if (color.Bold)
                    return color.Italic ? BoldItalicFont : BoldFont;
                return color.Italic ? ItalicFont : RegularFont;
            }
        }

        static TextFontContainer defaultFont = null;
        public static TextFontContainer Default
        {
            get { return defaultFont ?? (defaultFont = new TextFontContainer(SystemFonts.DefaultFont)); }
        }
    }
}
