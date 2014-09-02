using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyBones.UI.Controls.Tree
{
    public interface ITreeDataSource<TData> : IEnumerable<TData>
    {
        string KeyField { get; }
        string ParentKeyField { get; }
        object RootValue { get; }

        IEnumerable GetChildren(TreePath treePath);

        event EventHandler<TreeDataEventArgs> DataChanged;
        event EventHandler<TreeDataEventArgs> DataInserted;
        event EventHandler<TreeDataEventArgs> DataRemoved;
    }

    public class TreeDataEventArgs : TreePathEventArgs
    {
        private object[] _children;
        public object[] Children
        {
            get { return _children; }
        }

        private int[] _indices;
        public int[] Indices
        {
            get { return _indices; }
        }

        public TreeDataEventArgs(TreePath parent, object[] children)
            : this(parent, null, children)
        {
        }

        public TreeDataEventArgs(TreePath parent, int[] indices, object[] children)
            : base(parent)
        {
            if (children == null)
                throw new ArgumentNullException();

            if (indices != null && indices.Length != children.Length)
                throw new ArgumentException("indices and children arrays must have the same length");

            _indices = indices;
            _children = children;
        }
    }
}
