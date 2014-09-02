using System.Runtime.Serialization;
using System;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    [DataContract]
    [Table(BasicDataType.Department)]
    public class Department : Entity, IEquatable<Department>, IComparable<Department>
    {
        [DataMember]
        [Key]
        public virtual string Id { get; set; }
        [DataMember]
        public virtual string Name { get; set; }
        [DataMember]
        public virtual string FullName { get; set; }
        [DataMember]
        public virtual string Type { get; set; }
        [DataMember]
        public virtual string ParentDepart { get; set; }

        public virtual int CompareTo(Department other)
        {
            return string.Compare(Id, other.Id);
        }
        public virtual bool Equals(Department other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(Id, other.Id);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Department);
        }
        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
        public override string ToString()
        {
            return Id + "-" + Name;
        }
    }
}
