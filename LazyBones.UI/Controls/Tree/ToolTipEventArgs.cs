using System;

namespace LazyBones.UI.Controls.Tree
{
    public class ToolTipsEventArgs : EventArgs
    {
        public TreeListNode Node { get; private set; }
        public NodeCell Cell { get; private set; }
        public string ToolTips { get; set; }
        public bool Handled { get; set; }
        public ToolTipsEventArgs(TreeListNode node, NodeCell cell)
        {
            Node = node;
            Cell = cell;
        }
    }
}
