using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.TextEditor
{
    static class Helper
    {
        public static bool IsLetterDigitOrUnderscore(this char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
        public static bool IsWordPart(this char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }
        public static CharType GetCharType(this char c)
        {
            if (IsLetterDigitOrUnderscore(c))
                return CharType.LetterDigitOrUnderscore;
            if (Char.IsWhiteSpace(c))
                return CharType.WhiteSpace;
            return CharType.Other;
        }
        public static int GetFontHeight(this Font font)
        {
            var height1 = TextRenderer.MeasureText("_", font).Height;
            var height2 = (int)Math.Ceiling(font.GetHeight());
            return Math.Max(height1, height2) + 1;
        }
        public static Color Reverse(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }
    }
    public enum CharType
    {
        LetterDigitOrUnderscore,
        WhiteSpace,
        Other
    }
}
