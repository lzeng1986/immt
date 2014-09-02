using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Tree
{
    class NodeCellCollection : IList<NodeCell>
    {
        List<NodeCell> nodes = new List<NodeCell>();
        TreeList tree;
        public NodeCellCollection(TreeList tree)
        {
            this.tree = tree;
        }
        public int IndexOf(NodeCell item)
        {
            return nodes.IndexOf(item);
        }

        public void Insert(int index, NodeCell item)
        {
            ParamGuard.NotNull(item, "item");

            if (item.Tree != tree)
            {
                if (item.Tree != null)
                {
                    item.Tree.NodeControls.Remove(item);
                }
                nodes.Insert(index, item);
                item.AssignParentInternal(tree);
                tree.FullUpdate();
            }
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            item.AssignParentInternal(null);
            nodes.RemoveAt(index);
            tree.FullUpdate();
        }

        public NodeCell this[int index]
        {
            get { return nodes[index]; }
            set
            {
                using (tree.GetUpdateHolder())
                {
                    nodes.RemoveAt(index);
                    Insert(index, value);
                }
            }
        }

        public void Add(NodeCell item)
        {
            Insert(nodes.Count - 1, item);
        }

        public void Clear()
        {
            using (tree.GetUpdateHolder())
            {
                while (nodes.Count != 0)
                    nodes.RemoveAt(nodes.Count - 1);
            }
        }

        public bool Contains(NodeCell item)
        {
            return nodes.Contains(item);
        }

        public void CopyTo(NodeCell[] array, int arrayIndex)
        {
            nodes.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return nodes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(NodeCell item)
        {
            item.AssignParentInternal(null);
            var b = nodes.Remove(item);
            tree.FullUpdate();
            return b;
        }

        public IEnumerator<NodeCell> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }

    class NodeControlCollectionEditor : CollectionEditor
    {
        private Type[] _types;

        public NodeControlCollectionEditor(Type type)
            : base(type)
        {
            //_types = new Type[] { typeof(NodeTextBox), typeof(NodeComboBox), typeof(NodeCellCheckBox),
            //    typeof(NodeStateIcon), typeof(NodeCellImage)  };
        }

        protected override System.Type[] CreateNewItemTypes()
        {
            return _types;
        }
    }
}
