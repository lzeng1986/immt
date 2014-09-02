using System.Collections.Generic;

namespace LazyBones.Collection
{
    public interface IRBNodeHost<T>
    {
        void ChildChanged(RBNode<T> node);
        void LeftRotated(RBNode<T> node);
        void RightRotated(RBNode<T> node);
    }
}
