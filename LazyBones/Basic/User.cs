using System;
using System.Runtime.Serialization;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示系统用户
    /// </summary>
    [DataContract]
    [Table(BasicDataType.User)]
    public class User : Entity, IEquatable<User>, IComparable<User>
    {
        /// <summary>
        /// 获取或设置用户登录名
        /// </summary>
        [DataMember]
        [Key]
        public string UserName { get; set; }
        /// <summary>
        /// 获取或设置用户是否有效
        /// </summary>
        [DataMember]
        public virtual bool IsAvailable { get; set; }
        /// <summary>
        /// 比较该<see cref="User"/>对象是否与另一个<see cref="User"/>对象的大小
        /// </summary>
        /// <param name="other">比较的<see cref="User"/>对象</param>
        /// <returns>小于0，则小于；等于0，则等于；大于零，则大于</returns>
        public int CompareTo(User other)
        {
            return string.Compare(UserName, other.UserName);
        }
        /// <summary>
        /// 判断该<see cref="User"/>对象是否与另一个<see cref="User"/>对象相等
        /// </summary>
        /// <param name="other">比较的<see cref="User"/>对象</param>
        /// <returns>是否相等</returns>
        public bool Equals(User other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return string.Equals(UserName, other.UserName);
        }
        /// <summary>
        /// 重写object.Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }
        /// <summary>
        /// 重写object.GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return UserName == null ? 0 : UserName.GetHashCode();
        }
    }
}
