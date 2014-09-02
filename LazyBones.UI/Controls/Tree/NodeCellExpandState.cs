using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LazyBones.UI.Controls.Tree
{
    public class NodeCellExpandState : NodeCell
    {
        protected Image collapsedImg = ControlRes.plus;
        protected Image expandedImg = ControlRes.minus;

        public virtual int ImageSize { get { return 9; } }
        public virtual int Width { get { return 16; } }

        public override Size MeasureSize(TreeListNode node)
        {
            return new Size(Width, Width);
        }

        public sealed override void Draw(TreeListNode node, DrawContext context)
        {
            if (node.CanExpand)
            {
                Rectangle r = context.Bounds;
                int dy = (int)Math.Round((float)(r.Height - ImageSize) / 2);
                if (Application.RenderWithVisualStyles)
                {
                    VisualStyleRenderer renderer;
                    if (node.IsExpanded)
                        renderer = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Opened);
                    else
                        renderer = new VisualStyleRenderer(VisualStyleElement.TreeView.Glyph.Closed);
                    renderer.DrawBackground(context.Graphics, new Rectangle(r.X, r.Y + dy, ImageSize, ImageSize));
                }
                else
                {
                    var img = node.IsExpanded ? expandedImg : collapsedImg;
                    context.Graphics.DrawImageUnscaled(img, new Point(r.X, r.Y + dy));
                }
            }
        }

        public sealed override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                args.Handled = true;
                if (Owner.CanExpand)
                    Owner.IsExpanded = !args.Node.IsExpanded;
            }
        }

        public override void MouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
            args.Handled = true;
        }
    }
}
