using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    static class Helper
    {
        public static StringAlignment TranslateAligment(HorizontalAlignment aligment)
        {
            if (aligment == HorizontalAlignment.Left)
                return StringAlignment.Near;
            else if (aligment == HorizontalAlignment.Right)
                return StringAlignment.Far;
            else
                return StringAlignment.Center;
        }
        public static void FillAligment(ContentAlignment aligment, StringFormat format)
        {
            switch (aligment)
            {
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.MiddleCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Near;
                    break;
            }
        }
        public static TextFormatFlags TranslateAligment(ContentAlignment aligment)
        {
            switch (aligment)
            {
                case ContentAlignment.BottomCenter:
                    return TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomLeft:
                    return TextFormatFlags.Bottom | TextFormatFlags.Left;
                case ContentAlignment.BottomRight:
                    return TextFormatFlags.Bottom | TextFormatFlags.Right;
                case ContentAlignment.MiddleCenter:
                    return TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.MiddleLeft:
                    return TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                case ContentAlignment.MiddleRight:
                    return TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                case ContentAlignment.TopCenter:
                    return TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.TopLeft:
                    return TextFormatFlags.Top | TextFormatFlags.Left;
                case ContentAlignment.TopRight:
                    return TextFormatFlags.Top | TextFormatFlags.Right;
                default:
                    return TextFormatFlags.Default;
            }
        }
    }
}
