using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace LazyBones.Collection
{
    /// <summary>
    /// 表示强类型树形结构
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    public class TreeSet<T> : IEnumerable<T>
    {
        private TreeSetNode<T> root;
        public TreeSetNode<T> Root
        {
            get { return root; }
            set { root = value; }
        }
        public IEnumerator<T> GetEnumerator()
        {
            if (root != null)
            {
                yield return root.Val;
                foreach (var v in GetEnumeratorInternal(root))
                {
                    yield return v;
                }
            }
        }
        IEnumerable<T> GetEnumeratorInternal(TreeSetNode<T> node)
        {
            foreach (var c in root.children)
            {
                yield return c.Val;
            }
            foreach (var c in root.children)
            {
                foreach (var v in GetEnumeratorInternal(c))
                {
                    yield return v;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    /// <summary>
    /// 表示树形结构中的一个节点
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    public class TreeSetNode<T>
    {
        public TreeSetNode()
        {
        }
        public TreeSetNode(T val)
        {
            this.Val = val;
        }
        internal List<TreeSetNode<T>> children = new List<TreeSetNode<T>>();
        TreeSetNode<T> parent = null;
        public TreeSetNode<T> Parent
        {
            get { return parent; }
            set
            {
                if (parent == value)
                    return;
                if (parent != null)
                    parent.RemoveChild(this);
                parent = value;
                if (parent != null)
                    parent.AddChild(this);
            }
        }
        public T Val { get; set; }
        public void RemoveChild(T childVal)
        {
            var node = children.FirstOrDefault(t => t.Val.Equals(childVal));
            if (object.Equals(node , default(T)))
                return;
            RemoveChild(node);
        }
        public void RemoveChild(TreeSetNode<T> child)
        {
            children.Remove(child);
        }
        public void AddChild(T childVal)
        {
            AddChild(new TreeSetNode<T>(childVal));
        }
        public void AddChild(TreeSetNode<T> child)
        {
            if (children.Contains(child))
                return;
            children.Add(child);
        }
    }
}
