
namespace LazyBones.Scheduling.Core
{
    public class Key
    {
        readonly int hash;
        public Key(string name)
            : this(name, "default")
        { 
        }
        public Key(string name, string group)
        {
            var hash1 = string.IsNullOrEmpty(name) ? 0 : name.GetHashCode();
            var hash2 = string.IsNullOrEmpty(group) ? 0 : group.GetHashCode();
            hash = hash1 ^ hash2;
            Name = name;
            Group = group;
        }
        public string Name { get; private set; }
        public string Group { get; private set; }
        public override bool Equals(object obj)
        {
            var other = obj as Key;
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(Name, other.Name) && string.Equals(Group, other.Group);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
