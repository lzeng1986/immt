using System.Drawing;

namespace LazyBones.UI.Controls.Tree
{
    class NodeCellImage : NodeCell
    {
        public override Size MeasureSize(TreeListNode node)
        {
            var image = Image;
            if (image != null)
                return image.Size;
            else
                return Size.Empty;
        }

        public override void Draw(TreeListNode node, DrawContext context)
        {
            var image = Image;
            if (image != null)
            {
                var point = context.Bounds.Location;
                point.Offset(0,(context.Bounds.Height - image.Height) / 2);//靠左，上下居中
                context.Graphics.DrawImage(image, point);
            }
        }

        protected virtual Image Image
        {
            get { return Value as Image; }
        }
    }
}
