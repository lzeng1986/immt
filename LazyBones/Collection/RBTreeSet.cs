using System;
using System.Collections.Generic;
using LazyBones.Utils;

namespace LazyBones.Collection
{
    /// <summary>
    /// 表示强类型红黑树结构
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    public class RBTreeSet<T> : ICollection<T>
    {
        internal const bool Black = false, Red = true;
        IRBNodeHost<T> host;
        IComparer<T> comparer;
        RBNode<T> root;

        public RBTreeSet(IRBNodeHost<T> host)
            : this(host, Comparer<T>.Default)
        {
        }
        public RBTreeSet(IRBNodeHost<T> host, IComparer<T> comparer)
        {
            this.host = host;
            this.comparer = comparer;
        }
        public RBNode<T> Root { get { return root; } }
        int count = 0;
        //左旋操作（对x节点进行操作）：
        //   x               y
        //  / \             / \
        // a   y      =>   x   c    
        //    / \         / \
        //   b   c       a  b
        void RotateLeft(RBNode<T> x)
        {
            var y = x.Right;
            if (x.Parent == null)
            {
                root = y;
            }
            y.Parent = x.Parent;
            x.Parent = y;
            x.Right = y.Left;
            if (x.Right != null)
                x.Right.Parent = x;
            y.Left = x;
            host.LeftRotated(x);
            x.Update();
        }
        //右旋操作（对x节点进行操作）：
        //     x         y    
        //    / \       / \   
        //   y   c  => a   x   
        //  / \           / \ 
        // a  b          b   c
        void RotateRight(RBNode<T> x)
        {
            var y = x.Left;
            if (x.Parent == null)
            {
                root = y;
            }
            y.Parent = x.Parent;
            x.Parent = y;
            x.Left = y.Right;
            if (x.Left != null)
                x.Left.Parent = x;
            y.Right = x;
            host.RightRotated(x);
            x.Update();
        }

        public void Add(T item)
        {
            var node = new RBNode<T> { Val = item };
            var x = root;
            RBNode<T> y = null;
            while (x != null)
            {
                y = x;
                x = (comparer.Compare(item, x.Val) < 0) ? x.Left : x.Right;
            }
            node.Parent = y;
            if (y == null)
            {
                root = node;
            }
            else
            {
                if (comparer.Compare(item, y.Val) < 0)
                    y.Left = node;
                else
                    y.Right = node;
                host.ChildChanged(y);
                y.Update();
            }
            count++;
            InsertFixUp(node);
        }
        //根据《算法导论（原书第二版 中文版）》p167算法编写
        void InsertFixUp(RBNode<T> node)
        {
            while (true)
            {
                var parant = node.Parent;
                if (parant == null || parant.Color == Black)
                    break;
                var grandParent = parant.Parent;
                var uncle = (parant == grandParent.Left) ? grandParent.Right : grandParent.Left;
                if (uncle != null && uncle.Color == Red)
                {
                    parant.Color = uncle.Color = Black;
                    grandParent.Color = Red;
                    node = grandParent;
                }
                else if (parant == grandParent.Left && node == parant.Right)
                {
                    node = parant;
                    RotateLeft(node);
                }
                else if (parant == grandParent.Right && node == parant.Left)
                {
                    node = parant;
                    RotateRight(node);
                }
                else
                {
                    parant.Color = Black;
                    grandParent.Color = Red;
                    if (parant == grandParent.Left)
                        RotateRight(grandParent);
                    else
                        RotateLeft(grandParent);
                }
            }
        }

        public void Clear()
        {
            root = null;
            count = 0;
        }

        public bool Contains(T item)
        {
            var node = root;
            while (node != null)
            {
                var v = comparer.Compare(node.Val, item);
                if (v == 0)
                    return true;
                node = (v < 0) ? node.Left : node.Right;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ParamGuard.NotNull(array, "array");
            if (arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (root == null)
                return;

            var node = root.LeftMost;
            for (var i = 0; i < array.Length; i++, arrayIndex++)
            {
                if (node == null)
                    break;
                array[arrayIndex] = node.Val;
                node = node.Next();
            }
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public RBNode<T> Find(T item)
        {
            var node = LowerBound(item);
            while (node != null)
            {
                if (comparer.Compare(node.Val, item) == 0)
                    return node;
                node = node.Next();
            }
            return default(RBNode<T>);
        }

        public RBNode<T> LowerBound(T item)
        {
            var node = root;
            RBNode<T> resultNode = null;
            while (node != null)
            {
                if (comparer.Compare(node.Val, item) < 0)
                {
                    node = node.Right;
                }
                else
                {
                    resultNode = node;
                    node = node.Left;
                }
            }
            return resultNode;
        }

        public RBNode<T> UpperBound(T item)
        {
            var node = LowerBound(item);
            while (node != null && comparer.Compare(node.Val, item) == 0)
            {
                node = node.Next();
            }
            return node;
        }

        public bool Remove(T item)
        {
            var node = Find(item);
            if (node == null)
            {
                return false;
            }

            Remove(node);
            return true;
        }

        public void Remove(RBNode<T> node)
        {
            ParamGuard.NotNull(node, "node");

            var r = node;
            while (r.Parent != null)
                r = r.Parent;
            if (r != root)
                throw new ArgumentException("node不属于该RBTreeSet", "node");

            var y = (node.Left == null || node.Right == null) ? node : node.Next();
            var child = y.Left ?? y.Right;

            if (child != null)
                child.Parent = y.Parent;

            if (y.Parent == null)
            {
                root = child;
            }
            else
            {
                if (y == y.Parent.Left)
                    y.Parent.Left = child;
                else
                    y.Parent.Right = child;
                y.Parent.Update();
                host.ChildChanged(y.Parent);
            }

            if (y != node)
            {
                y.Left = node.Left;
                y.Right = node.Right;
                y.Parent = node.Parent;
                node.Parent.Update();
                host.ChildChanged(node.Parent);
            }

            if (y.Color == Black)
            {
                DeleteFixUp(child);
            }

            node.Left = node.Right = node.Parent = null; //将node从RBTreeSet删除
            count--;
        }
        //根据《算法导论（原书第二版 中文版）》p173算法编写
        void DeleteFixUp(RBNode<T> node)
        {
            while (node != root && node.Color == Black)
            {
                var parent = node.Parent;
                var isLeft = (node == node.Parent.Left);
                var uncle = isLeft ? node.Parent.Right : node.Parent.Left;

                if (IsRed(uncle))    //case1
                {
                    uncle.Color = Black;
                    parent.Color = Red;
                    if (isLeft)
                    {
                        RotateLeft(parent);
                        uncle = parent.Right;
                    }
                    else
                    {
                        RotateRight(parent);
                        uncle = parent.Left;
                    }
                }
                if (IsBlack(uncle.Left) && IsBlack(uncle.Right))//case 2
                {
                    uncle.Color = Red;
                    node = node.Parent;
                }
                else
                {
                    if (isLeft)
                    {
                        if (IsBlack(uncle.Right))//case 3
                        {
                            uncle.Left.Color = Black;
                            uncle.Color = Red;
                            RotateRight(uncle);
                            uncle = parent.Right;
                        }
                        uncle.Color = parent.Color;//case4
                        parent.Color = Black;
                        uncle.Right.Color = Black;
                        RotateLeft(parent);
                    }
                    else
                    {
                        if (IsBlack(uncle.Left))
                        {
                            uncle.Right.Color = Black;
                            uncle.Color = Red;
                            RotateLeft(uncle);
                            uncle = parent.Left;
                        }
                        uncle.Color = parent.Color;
                        parent.Color = Black;
                        uncle.Left.Color = Black;
                        RotateLeft(parent);
                    }
                    node = root;
                }
            }
        }

        static bool IsBlack(RBNode<T> node)
        {
            return node == null || node.Color == Black;
        }
        static bool IsRed(RBNode<T> node)
        {
            return node != null && node.Color == Red;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new RBTreeEnumerator<T>(Root.LeftMost);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDuplexEnumerator<T> Head
        {
            get
            {
                if (Root == null)
                    return null;
                return new RBTreeEnumerator<T>(Root.LeftMost);
            }
        }
        public IDuplexEnumerator<T> Tail
        {
            get
            {
                if (Root == null)
                    return null;
                return new RBTreeEnumerator<T>(Root.RightMost);
            }
        }
    }
}
