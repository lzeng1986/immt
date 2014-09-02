using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeListNode
    {
        public bool IsLeaf { get; internal set; }
        public int RowIndex { get; internal set; }
        TreeList tree;
        public TreeList Tree
        {
            get { return tree; }
        }

        public object Value { get; set; }
        public object Tag { get; set; }
        TreeListNode parentNode;
        public TreeListNode ParentNode
        {
            get { return parentNode; }
            internal set
            {
                parentNode = value;
                Level = parentNode == null ? 0 : parentNode.Level;
            }
        }

        public int Level { get; private set; }

        List<TreeListNode> childNodes = new List<TreeListNode>();
        public IList<TreeListNode> ChildNodes
        {
            get { return childNodes; }
        }
        public IEnumerable<TreeListNode> AllExpandedChildNodes
        {
            get
            {
                return isExpanded ?
                    childNodes.Concat(childNodes.SelectMany(c => c.AllExpandedChildNodes)) :
                    Enumerable.Empty<TreeListNode>();
            }
        }
        public IEnumerable<TreeListNode> AllChildNodes
        {
            get { return childNodes.SelectMany(r => r.AllChildNodes); }
        }
        public TreeListNode NextNode
        {
            get
            {
                if (ParentNode != null)
                {
                    var index = ParentNode.childNodes.IndexOf(this);
                    if (index + 1 < ParentNode.childNodes.Count)
                        return ParentNode.childNodes[index + 1];
                }
                return null;
            }
        }

        internal TreeListNode Next
        {
            get { return RowIndex < tree.VisibleNodes.Count - 1 ? tree.VisibleNodes[RowIndex + 1] : null; }
        }

        public bool CanExpand
        {
            get { return (childNodes.Count > 0 || (!HasChildLoaded && !IsLeaf)); }
        }

        public bool HasChildLoaded { get; internal set; }

        bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    if (value)
                        tree.OnNodeExpanding(this);
                    else
                        tree.OnNodeCollapsing(this);

                    if (value && !HasChildLoaded)
                    {
                        LoadChild();
                    }
                    isExpanded = value; //&& CanExpand;
                    if (isExpanded == value)
                        tree.SmartFullUpdate();
                    else
                        tree.UpdateView();

                    if (value)
                        tree.OnNodeExpanded(this);
                    else
                        tree.OnNodeCollapsed(this);
                }
            }
        }

        bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value)
                    return;
                isSelected = value;
                if (tree.IsMyNode(this))
                {
                    if (isSelected)
                    {
                        if (!tree.SelectedNodesInternal.Contains(this))
                            tree.SelectedNodesInternal.Add(this);

                        if (tree.SelectedNodesInternal.Count == 1)
                            tree.CurrentNode = this;
                    }
                    else
                        tree.SelectedNodesInternal.Remove(this);
                    tree.UpdateView();
                    tree.OnSelectionChanged();
                }
            }
        }

        public void ExpandAll()
        {
            IsExpanded = true;
            childNodes.ForEach(n => n.ExpandAll());
        }

        public void CollapseAll()
        {
            IsExpanded = false;
            childNodes.ForEach(n => n.CollapseAll());
        }

        public void LoadChild()//强制加载子节点
        {
            var oldCursor = Tree.Cursor;
            tree.Cursor = Cursors.WaitCursor;
            try
            {
                tree.LoadChildNodes(this);
            }
            finally
            {
                tree.Cursor = oldCursor;
            }
        }

        public TreePath Path
        {
            get
            {
                if (this.Level == 0)
                    return TreePath.Empty;
                else
                {
                    var node = this;
                    var p = new List<object>() { node.Value };
                    while (node.Level > 0)
                    {
                        node = node.ParentNode;
                        p.Add(node.Value);
                    }
                    p.Reverse();
                    return new TreePath(p.ToArray());
                }
            }
        }

        internal TreeListNode(TreeList tree, object value)
        {
            ParamGuard.NotNull(tree, "tree");
            IsLeaf = false;
            RowIndex = -1;
            this.tree = tree;
            Value = value;
        }
    }
}
