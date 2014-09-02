using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LazyBones.Collection;

namespace LazyBones.UI.Controls.TextEditor
{
    class TextLineTree : IList<TextLine>, IRBNodeHost<TextLine>
    {
        RBTreeSet<TextLine> rbTree;
        public TextLineTree()
        {
            rbTree = new RBTreeSet<TextLine>(this);
        }
        public void ChildChanged(RBNode<TextLine> node)
        {
            Debug.Assert(node != null);

            if (node.Val.Node == null)
                node.Val.Node = node;

            node.Val.LineNo = GetNodeIndex(node);

            int totalLength = node.Val.Length;
            if (node.Left != null)
            {
                totalLength += node.Left.Val.TotalLength;
            }
            if (node.Right != null)
            {
                totalLength += node.Right.Val.TotalLength;
            }
            if (totalLength != node.Val.TotalLength)
            {
                node.Val.TotalLength = totalLength;
                if (node.Parent != null)
                    ChildChanged(node.Parent);
            }
        }

        public void LeftRotated(RBNode<TextLine> node)
        {
            ChildChanged(node);
            ChildChanged(node.Parent);
        }

        public void RightRotated(RBNode<TextLine> node)
        {
            ChildChanged(node);
            ChildChanged(node.Parent);
        }

        public int Compare(TextLine x, TextLine y)
        {
            if (x == null)
                return y == null ? 0 : -1;
            return y == null ? 1 : x.Offset.CompareTo(y.Offset);
        }

        // 获取节点的序号，从最左下节点开始计算
        int GetNodeIndex(RBNode<TextLine> node)
        {
            var ind = node.Left != null ? node.Left.Count : 0;
            while (node.Parent != null)
            {
                if (node == node.Parent.Right)
                {
                    if (node.Left != null)
                        ind += node.Left.Count;
                    ind++;
                }
                node = node.Parent;
            }
            return ind;
        }
        internal RBNode<TextLine> GetNodebyOffset(int offset)
        {
            if (offset < 0 || TotalLength < offset)
                throw new ArgumentOutOfRangeException("offset");
            if (offset == TotalLength)
                return rbTree.Root.RightMost;
            var node = rbTree.Root;
            while (true)
            {
                if (node.Left != null && offset < node.Left.Val.TotalLength)
                {
                    node = node.Left;
                }
                else
                {
                    if (node.Left != null)
                        offset -= node.Left.Val.TotalLength;
                    offset -= node.Val.TotalLength;
                    if (offset < 0)
                        return node;
                    node = node.Right;
                }
            }
        }
        internal RBNode<TextLine> GetNode(int index)
        {
            if (index < 0 || rbTree.Count <= index)
                throw new ArgumentOutOfRangeException("index");
            var node = rbTree.Root;
            while (true)
            {
                if (node.Left != null && index < node.Left.Count)
                {
                    node = node.Left;
                }
                else
                {
                    if (node.Left != null)
                        index -= node.Left.Count;
                    if (index == 0)
                        return node;
                    index--;
                    node = node.Right;
                }
            }
        }
        public int TotalLength
        {
            get { return rbTree.Root == null ? 0 : rbTree.Root.Val.TotalLength; }
        }
        public int IndexOf(TextLine item)
        {
            var ind = item.LineNo;
            if (ind < 0 || rbTree.Count <= ind || item != this[ind])
                return -1;
            return ind;
        }

        public void Insert(int index, TextLine item)
        {
            Add(item);
        }

        public void RemoveAt(int index)
        {
            rbTree.Remove(GetNode(index));
        }

        public TextLine this[int index]
        {
            get { return GetNode(index).Val; }
            set { throw new NotImplementedException(); }
        }

        public void Add(TextLine item)
        {
            rbTree.Add(item);
        }

        public void Clear()
        {
            rbTree.Clear();
        }

        public bool Contains(TextLine item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(TextLine[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            foreach (var v in this)
            {
                array[arrayIndex++] = v;
            }
        }

        public int Count
        {
            get { return rbTree.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TextLine item)
        {
            return rbTree.Remove(item);
        }

        public IEnumerator<TextLine> GetEnumerator()
        {
            return rbTree.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void SetLineLength(TextLine line, int newLength)
        {
            line.Length = newLength;
            ChildChanged(line.Node);
        }
    }
}
