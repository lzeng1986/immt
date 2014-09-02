using System;
using System.Runtime.Serialization;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示角色与可访问资源的一一对应关系
    /// </summary>
    [DataContract]
    [Table(BasicDataType.RoleRes)]
    public class RoleRes : Entity, IEquatable<RoleRes>, IComparable<RoleRes>
    {
        /// <summary>
        /// 获取或设置<see cref="RoleRes"/>对象关联的角色
        /// </summary>
        [DataMember]
        public string RoleName { get; set; }
        /// <summary>
        /// 获取或设置<see cref="RoleRes"/>对象关联的资源
        /// </summary>
        [DataMember]
        public string Resource { get; set; }
        /// <summary>
        /// 获取该<see cref="RoleRes"/>对象是否与另外<see cref="RoleRes"/>对象相等
        /// </summary>
        /// <param name="other">比较的<see cref="RoleRes"/>对象</param>
        /// <returns>是否相等</returns>
        public bool Equals(RoleRes other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(RoleName, other.Resource) && string.Equals(RoleName, other.Resource);
        }
        /// <summary>
        /// 比较该<see cref="RoleRes"/>对象与另外<see cref="RoleRes"/>对象的大小
        /// </summary>
        /// <param name="other">比较的<see cref="RoleRes"/>对象</param>
        /// <returns>小于0，则小于；等于0，则等于；大于零，则大于</returns>
        public int CompareTo(RoleRes other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            return string.Concat(RoleName, Resource).CompareTo(string.Concat(other.RoleName, other.Resource));
        }
    }
}
