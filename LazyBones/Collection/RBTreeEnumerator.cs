using System;

namespace LazyBones.Collection
{
    //实现一个RBTree迭代器，并且支持向前移动
    internal class RBTreeEnumerator<T> : IDuplexEnumerator<T>
    {
        RBNode<T> node;
        public RBTreeEnumerator(RBNode<T> node)
        {
            this.node = node;
        }
        public T Current
        {
            get { return node != null ? node.Val : default(T); }
        }

        public void Dispose()
        {
            node = null;
            GC.SuppressFinalize(this);
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()  //红黑树上向后移动一个节点
        {
            if (node == null)
                return false;
            node = node.Next();
            return node != null;
        }
        public bool MovePrev()  //红黑树上向前移动一个节点
        {
            if (node == null)
                return false;
            node = node.Prev();
            return node != null;
        }
        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
