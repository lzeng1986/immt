using System;
using System.Runtime.Serialization;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示组织架构中的人员
    /// </summary>
    [DataContract]
    [Table(BasicDataType.Employee)]
    public class Employee : Entity, IEquatable<Employee>, IComparable<Employee>
    {
        /// <summary>
        /// 获取或设置人员Id
        /// </summary>
        [DataMember]
        [Key]
        public virtual string Id { get; set; }
        /// <summary>
        /// 获取或设置人员姓名
        /// </summary>
        [DataMember]
        public virtual string Name { get; set; }
        /// <summary>
        /// 获取或设置人员创建时间
        /// </summary>
        [DataMember]
        public virtual DateTime CreateTime { get; set; }
        /// <summary>
        /// 获取或设置人员是否有效
        /// </summary>
        [DataMember]
        public virtual bool IsAvailable { get; set; }
        /// <summary>
        /// 获取或设置人员类型
        /// </summary>
        [DataMember]
        public virtual string Type { get; set; }
        /// <summary>
        /// 获取或设置人员常驻部门
        /// </summary>
        [DataMember]
        public virtual string FirstDepartment { get; set; }
        /// <summary>
        /// 获取或设置人员临时部门列表
        /// </summary>
        [DataMember]
        public virtual string[] SecondaryDeparts { get; set; }

        public virtual int CompareTo(Employee other)
        {
            return Id.CompareTo(other.Id);
        }

        public virtual bool Equals(Employee other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Employee);
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
