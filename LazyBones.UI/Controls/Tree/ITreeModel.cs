using System;
using System.Collections;

namespace LazyBones.UI.Controls.Tree
{
    public interface ITreeModel
    {
        IEnumerable GetChildren(TreePath treePath);
        bool IsLeaf(TreePath treePath);

        event EventHandler<TreeDataEventArgs> NodesChanged;
        event EventHandler<TreeDataEventArgs> NodesInserted;
        event EventHandler<TreeDataEventArgs> NodesRemoved;
        event EventHandler<TreePathEventArgs> StructureChanged;
    }

    
}
