using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Collection
{
    /// <summary>
    /// 表示强类型二叉树结构
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    public class BinaryTreeSet<T> : IList<T>
    {
        public BinaryTreeSet()
            : this(EqualityComparer<T>.Default)
        { }
        public BinaryTreeSet(IEqualityComparer<T> comparer)
        {
        }
        T root = default(T);

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    internal class BinaryTreeNode<T>
    {
        private BinaryTreeNode<T> parent;
        public BinaryTreeNode<T> Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        private BinaryTreeNode<T> left;
        public BinaryTreeNode<T> Left
        {
            get { return left; }
            set { left = value; }
        }
        private BinaryTreeNode<T> right;
        public BinaryTreeNode<T> Right
        {
            get { return right; }
            set { right = value; }
        }
        public BinaryTreeNode<T> LeftMost
        {
            get
            {
                var node = this;
                while (node.left != null)
                {
                    node = node.left;
                }
                return node;
            }
        }
        public BinaryTreeNode<T> RightMost
        {
            get
            {
                var node = this;
                while (node.right != null)
                {
                    node = node.right;
                }
                return node;
            }
        }
    }
}
