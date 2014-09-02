
using System.Collections.Generic;
namespace LazyBones.UI.Controls.TextEditor
{
    //实现联合主键
    class PairKey<T1, T2> : System.IEquatable<PairKey<T1, T2>>
    {
        T1 v1;
        T2 v2;
        public PairKey(T1 v1, T2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PairKey<T1, T2>);
        }
        public override int GetHashCode()
        {
            var hash1 = v1.Equals(default(T1)) ? 0 : v1.GetHashCode();
            var hash2 = v2.Equals(default(T1)) ? 0 : v1.GetHashCode();
            return hash1 ^ hash2;
        }
        public bool Equals(PairKey<T1, T2> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            return EqualityComparer<T1>.Default.Equals(v1, other.v1) && EqualityComparer<T2>.Default.Equals(v2, other.v2);
        }
    }
}
