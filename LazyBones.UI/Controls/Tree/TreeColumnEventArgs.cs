using System;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeColumnEventArgs : EventArgs
    {
        public TreeColumn Column { get; private set; }
        public TreeColumnEventArgs(TreeColumn column)
        {
            Column = column;
        }
    }
}
