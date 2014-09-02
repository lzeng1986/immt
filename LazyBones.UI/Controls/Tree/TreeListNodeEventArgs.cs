using System;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeListNodeEventArgs : EventArgs
    {
        public TreeListNode Row { get; private set; }
        public TreeListNodeEventArgs(TreeListNode row)
        {
            Row = row;
        }
    }
}
