using System;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Tree
{
    public class TreePathEventArgs : EventArgs
    {
        public TreePath Path { get; private set; }

        public TreePathEventArgs()
            : this(TreePath.Empty)
        {
        }

        public TreePathEventArgs(TreePath path)
        {
            ParamGuard.NotNull(path, "path");
            Path = path;
        }
    }
}
