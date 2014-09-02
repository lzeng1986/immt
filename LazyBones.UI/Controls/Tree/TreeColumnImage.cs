using System.Drawing;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeColumnImage : TreeColumn
    {
        public override Size MeasureSize(TreeListNode node)
        {
            var image = GetImage(node);
            if (image != null)
                return image.Size;
            else
                return Size.Empty;
        }

        public override void Draw(TreeListNode node, DrawContext context)
        {
            var image = GetImage(node);
            if (image != null)
            {
                Point point = new Point(context.Bounds.X,
                    context.Bounds.Y + (context.Bounds.Height - image.Height) / 2);
                context.Graphics.DrawImage(image, point);
            }
        }

        protected virtual Image GetImage(TreeListNode node)
        {
            return node.Value as Image;
        }
    }
}
