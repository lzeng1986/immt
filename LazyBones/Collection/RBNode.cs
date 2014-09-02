
namespace LazyBones.Collection
{
    public class RBNode<T>
    {
        public RBNode<T> Left { get; internal set; }
        public RBNode<T> Right { get; internal set; }
        public RBNode<T> Parent { get; internal set; }
        public T Val { get; internal set; }

        internal static readonly RBNode<T> Nil = new RBNode<T>() { Color = RBTreeSet<T>.Black };

        internal bool Color = true;

        int count = 1;
        /// <summary>
        /// 以此节点为根节点的红黑树所有节点数量，包含此节点
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        public RBNode<T> LeftMost
        {
            get
            {
                var node = this;
                while (node.Left != null)
                {
                    node = node.Left;
                }
                return node;
            }
        }
        public RBNode<T> RightMost
        {
            get
            {
                var node = this;
                while (node.Right != null)
                {
                    node = node.Right;
                }
                return node;
            }
        }
        /// <summary>
        /// 后移一个的节点
        /// </summary>
        public RBNode<T> Next()
        {
            if (this.Right != null)
            {
                return this.Right.LeftMost;
            }

            var p = this.Parent;
            var node = this;
            while (p != null && p.Right == node)//从父节点的右端逐步上移
            {
                node = p;
                p = node.Parent;
            }

            return node;
        }
        /// <summary>
        /// 前移一个的节点
        /// </summary>
        public RBNode<T> Prev()
        {
            if (this.Left != null)
            {
                return this.Left.RightMost;
            }

            var p = this.Parent;
            var node = this;
            while (p != null && p.Left == node)//从父节点的左端逐步上移
            {
                node = p;
                p = node.Parent;
            }

            return node;
        }

        internal void Update()
        {
            var c = 1;
            if (Left != null)
                c += Left.count;
            if (Right != null)
                c += Right.count;
            if (c != count) {
                count = c;
                if (Parent != null)
                    Parent.Update();
            }
        }
    }
}
