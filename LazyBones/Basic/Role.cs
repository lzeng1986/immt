using System.Runtime.Serialization;
using System;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示用户角色
    /// </summary>
    [DataContract]
    [Table(BasicDataType.Role)]
    public class Role : Entity, IEquatable<Role>, IComparable<Role>
    {
        [DataMember]
        [Key]
        public virtual string Name { get; set; }
        [DataMember]
        public virtual string Description { get; set; }

        public virtual int CompareTo(Role other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            return string.Compare(Name, other.Name);
        }

        public virtual bool Equals(Role other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(Name, other.Name);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Role);
        }
        public override int GetHashCode()
        {
            return Name == null ? 0 : Name.GetHashCode();
        }
    }
}
