using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.Extensions
{
    public static class Extensions
    {
        public static StringAlignment ToStringAlignment(this HorizontalAlignment aligment)
        {
            if (aligment == HorizontalAlignment.Left)
                return StringAlignment.Near;
            else if (aligment == HorizontalAlignment.Right)
                return StringAlignment.Far;
            else
                return StringAlignment.Center;
        }
        public static HorizontalAlignment ToHorizontalAlignment(StringAlignment aligment)
        {
            if (aligment == StringAlignment.Near)
                return HorizontalAlignment.Left;
            else if (aligment == StringAlignment.Far)
                return HorizontalAlignment.Right;
            else
                return HorizontalAlignment.Center;
        }
        public static TextFormatFlags ToTextFormatFlags(this ContentAlignment aligment)
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
        public static ContentAlignment ToContentAlign(this StringFormat format)
        {
            switch (format.Alignment)
            {
                case StringAlignment.Center:
                    switch (format.LineAlignment)
                    {
                        case StringAlignment.Center:
                            return ContentAlignment.MiddleCenter;
                        case StringAlignment.Far:
                            return ContentAlignment.BottomCenter;
                        default:
                            return ContentAlignment.TopCenter;
                    }
                case StringAlignment.Far:
                    switch (format.LineAlignment)
                    {
                        case StringAlignment.Center:
                            return ContentAlignment.MiddleRight;
                        case StringAlignment.Far:
                            return ContentAlignment.BottomRight;
                        default:
                            return ContentAlignment.TopRight;
                    }
                default://StringAlignment.Near
                    switch (format.LineAlignment)
                    {
                        case StringAlignment.Center:
                            return ContentAlignment.MiddleLeft;
                        case StringAlignment.Far:
                            return ContentAlignment.BottomLeft;
                        default:
                            return ContentAlignment.TopLeft;
                    }
            }
        }
        public static StringFormat FillAligment(this StringFormat format, ContentAlignment aligment)
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
            return format;
        }
    }
}
