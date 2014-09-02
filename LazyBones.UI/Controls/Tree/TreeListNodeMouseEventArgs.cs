using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeListNodeMouseEventArgs : MouseEventArgs
    {
        public TreeListNode Node { get; internal set; }
        public NodeCell Cell { get; internal set; }
        public Point AbsoluteLocation { get; internal set; }
        public bool Handled { get; internal set; }
        public Rectangle CellBounds { get; internal set; }

        public TreeListNodeMouseEventArgs(MouseEventArgs args)
            : base(args.Button, args.Clicks, args.X, args.Y, args.Delta)
        {
        }
    }
}
