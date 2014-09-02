using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeColumnExpandState : TreeColumn
    {
        public TreeColumnExpandState()
        {
            CollapsedImg = ControlRes.plus;
            ExpandedImg = ControlRes.minus;
        }

        [Browsable(false)]
        public virtual int ImageSize { get { return 9; } }
        [Category("Appearance")]
        public Image CollapsedImg { get; set; }
        [Category("Appearance")]
        public Image ExpandedImg { get; set; }

        bool checkBoxVisible = false;
        [DefaultValue(false), Category("Behavior")]
        public bool CheckBoxVisible
        {
            get { return checkBoxVisible; }
            set { checkBoxVisible = value; }
        }

        public override Size MeasureSize(TreeListNode node)
        {
            return new Size(Width, Width);
        }

        public override void Draw(TreeListNode node, DrawContext context)
        {
            if (node.CanExpand)
            {
                var rect = context.Bounds;
                int dy = (int)Math.Round((float)(rect.Height - ImageSize) / 2);
                var img = node.IsExpanded ? CollapsedImg : ExpandedImg;
                context.Graphics.DrawImageUnscaled(img, new Point(rect.X, rect.Y + dy));
            }
        }
        public sealed override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                args.Handled = true;
                if (args.Node.CanExpand)
                    args.Node.IsExpanded = !args.Node.IsExpanded;
            }
        }
        public override void MouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
            args.Handled = true;
        }
    }
}
