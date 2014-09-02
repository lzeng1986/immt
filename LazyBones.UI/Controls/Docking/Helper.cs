using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LazyBones.Extensions;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 辅助类，定义扩展方法，提供辅助功能
    /// </summary>
    internal static class Helper
    {
        public static bool IsValid(this DockStyle dockStyle, DockAreas dockAreas)
        {
            switch (dockStyle)
            {
                case DockStyle.Left:
                    return dockAreas.HasFlag(DockAreas.Left);
                case DockStyle.Top:
                    return dockAreas.HasFlag(DockAreas.Top);
                case DockStyle.Right:
                    return dockAreas.HasFlag(DockAreas.Right);
                case DockStyle.Bottom:
                    return dockAreas.HasFlag(DockAreas.Bottom);
                case DockStyle.None:
                    return dockAreas.HasFlag(DockAreas.Float);
                case DockStyle.Fill:
                    return dockAreas.HasFlag(DockAreas.Document);
                default:
                    return false;
            }
        }
        public static Point RtlTransformPoint(this Control control, Point point)
        {
            if (control.RightToLeft != RightToLeft.Yes)
                return point;
            else
                return new Point(control.Right - point.X, point.Y);
        }
        public static Rectangle RtlTransformRect(this Control control, Rectangle rect)
        {
            if (control.RightToLeft != RightToLeft.Yes)
                return rect;
            else
                return new Rectangle(control.Right - rect.Right, rect.Top, rect.Width, rect.Height);
        }
        const int curveSize = 6;
        public static GraphicsPath GetUpRoundCorner(this Rectangle rect)
        {
            var path = new GraphicsPath();
            path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top - curveSize / 2);
            path.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
            path.AddLine(rect.Left + curveSize / 2, rect.Top, rect.Right - curveSize / 2, rect.Top);
            path.AddArc(new Rectangle(rect.Right - curveSize, rect.Top, curveSize, curveSize), -90, 90);
            path.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Bottom);
            return path;
        }
        public static GraphicsPath GetDownRoundCorner(this Rectangle rect)
        {
            var path = new GraphicsPath();
            path.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom - curveSize / 2);
            path.AddArc(new Rectangle(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize), 0, 90);
            path.AddLine(rect.Right - curveSize / 2, rect.Bottom, rect.Left + curveSize / 2, rect.Bottom);
            path.AddArc(new Rectangle(rect.Left, rect.Bottom - curveSize, curveSize, curveSize), 90, 90);
            path.AddLine(rect.Left, rect.Bottom - curveSize / 2, rect.Left, rect.Top);
            return path;
        }

        public static GraphicsPath CalculateGraphicsPath(this Bitmap bitmap)
        {
            return bitmap.CalculateGraphicsPath(Color.Empty);
        }

        // From http://edu.cnzz.cn/show_3281.html
        public static GraphicsPath CalculateGraphicsPath(this Bitmap bitmap, Color transparentColor)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            if (transparentColor == Color.Empty)
                transparentColor = bitmap.GetPixel(0, 0);

            for (int row = 0; row < bitmap.Height; row++)
            {
                int colOpaquePixel = 0;
                for (int col = 0; col < bitmap.Width; col++)
                {
                    if (bitmap.GetPixel(col, row) != transparentColor)
                    {
                        colOpaquePixel = col;
                        int colNext = col;
                        for (colNext = colOpaquePixel; colNext < bitmap.Width; colNext++)
                            if (bitmap.GetPixel(colNext, row) == transparentColor)
                                break;

                        graphicsPath.AddRectangle(new Rectangle(colOpaquePixel, row, colNext - colOpaquePixel, 1));
                        col = colNext;
                    }
                }
            }
            return graphicsPath;
        }

        public static int GetHorizontal(this ScrollableControl.DockPaddingEdges padding)
        {
            ParamGuard.NotNull(padding, "padding");
            return padding.Left + padding.Right;
        }
        public static int GetVertical(this ScrollableControl.DockPaddingEdges padding)
        {
            ParamGuard.NotNull(padding, "padding");
            return padding.Top + padding.Bottom;
        }
    }
}
