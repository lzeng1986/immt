using LazyBones.Data;
using System.Runtime.Serialization;
using System;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示一个用户角色对象
    /// </summary>
    [DataContract]
    [Table(BasicDataType.UserRole)]
    public class UserRole : Entity, IEquatable<UserRole>, IComparable<UserRole>
    {
        /// <summary>
        /// 获取或设置<see cref="UserRole"/>对象关联的用户名
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 获取或设置<see cref="UserRole"/>对象关联的角色
        /// </summary>
        [DataMember]
        public string RoleName { get; set; }

        public bool Equals(UserRole other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(UserName, other.UserName) && string.Equals(RoleName, other.RoleName);
        }

        public int CompareTo(UserRole other)
        {
            if (ReferenceEquals(other, null))
                return 1;
            return string.Concat(UserName, RoleName).CompareTo(string.Concat(other.UserName,other.RoleName));
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as UserRole);
        }
        public override int GetHashCode()
        {
            var hash1 = UserName == null ? 0 : UserName.GetHashCode();
            var hash2 = RoleName == null ? 0 : RoleName.GetHashCode();
            return hash1 ^ hash2;
        }
    }
}
