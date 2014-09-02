
namespace LazyBones
{
    public class Pair
    {
        public readonly object First;
        public readonly object Second;
        public Pair(object first, object second)
        {
            First = first;
            Second = second;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Pair;
            if(other == null)
                return false;
            return First.Equals(other.First) && Second.Equals(other.Second);
        }
        public override int GetHashCode()
        {
            return unchecked(87 * First.GetHashCode() ^ Second.GetHashCode());
        }
    }
    public class Pair<TFirst, TSecond>
    {
        public readonly TFirst First;
        public readonly TSecond Second;
        public Pair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Pair<TFirst, TSecond>;
            if (other == null)
                return false;
            return First.Equals(other.First) && Second.Equals(other.Second);
        }
        public override int GetHashCode()
        {
            return unchecked(87 * First.GetHashCode() ^ Second.GetHashCode());
        }
    }
    public class Triplet
    {
        public readonly object First;
        public readonly object Second;
        public readonly object Third;
        public Triplet(object first, object second, object third)
        {
            First = first;
            Second = second;
            Third = third;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Triplet;
            if (other == null)
                return false;
            return First.Equals(other.First) && Second.Equals(other.Second) && Third.Equals(other.Third);
        }
        public override int GetHashCode()
        {
            return unchecked(87 * First.GetHashCode() ^ Second.GetHashCode() ^ Third.GetHashCode());
        }
    }
    public class Triplet<TFirst, TSecond, TThird>
    {
        public readonly TFirst First;
        public readonly TSecond Second;
        public readonly TThird Third;
        public Triplet(TFirst first, TSecond second, TThird third)
        {
            First = first;
            Second = second;
            Third = third;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Triplet;
            if (other == null)
                return false;
            return First.Equals(other.First) && Second.Equals(other.Second) && Third.Equals(other.Third);
        }
        public override int GetHashCode()
        {
            return unchecked(87 * First.GetHashCode() ^ Second.GetHashCode() ^ Third.GetHashCode());
        }
    }
}
