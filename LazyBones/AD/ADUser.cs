using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace LazyBones.AD
{
    /// <summary>
    /// 表示AD域用户，包括了用户的一些基本信息及状态
    /// </summary>
    [Author("曾樑")]
    [Serializable]
    [ADSchema("user")]
    public class ADUser
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [ADProperty("sAMAccountName")]
        public string UserName { get; internal set; }
        /// <summary>
        /// 用户的部门
        /// </summary>
        [ADProperty("department")]
        public string Department { get; internal set; }
        /// <summary>
        /// 用户的显示名
        /// </summary>
        [ADProperty("displayName")]
        public string DisplayName { get; internal set; }
        /// <summary>
        /// 用户全名
        /// </summary>
        [ADProperty("UserPrincipalName")]
        public string UserPrincipalName { get; internal set; }
        /// <summary>
        /// 电子邮件地址
        /// </summary>
        [ADProperty("mail")]
        public string Email { get; internal set; }
        /// <summary>
        /// 邮件的昵称
        /// </summary>
        [ADProperty("mailNickname")]
        public string MailNickname { get; internal set; }
        /// <summary>
        /// 用户是否为激活状态，即是否没有被禁用
        /// </summary>
        [ADProperty("mailNickname")]
        public bool Actived { get; internal set; }
        /// <summary>
        /// 用户是否为锁定
        /// </summary>
        [DirectoryProperty("IsAccountLocked")]
        public bool Locked { get; internal set; }
        /// <summary>
        /// 账户过期时间
        /// </summary>
        [ADProperty("AccountExpirationDate")]
        public DateTime AccountExpirationDate { get; internal set; }
        /// <summary>
        /// 最近一次设置密码时间
        /// </summary>
        [ADProperty("PasswordLastChanged")]
        public DateTime PasswordLastSet { get; internal set; }
        /// <summary>
        /// 用户当前的权限
        /// </summary>
        [ADProperty("userAccountControl")]
        public ADAccountFlags AccountControl { get; internal set; }
        /// <summary>
        /// 属于的组
        /// </summary>
        [ADProperty("memberOf")]
        public string[] Groups { get; internal set; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ADPropertyAttribute:Attribute
    {
        public string Name{get;private set;}
        public ADPropertyType PropertyType { get; private set; }
        public ADPropertyAttribute(string name)
            : this(name, ADPropertyType.Ldap)
        {
        }
        public ADPropertyAttribute(string name,ADPropertyType type)
        {
            Name = name;
            PropertyType = type;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    class ADSchemaAttribute : Attribute
    {
        public string Schema{get;private set;}
        public ADSchemaAttribute(string schema)
        {
            Schema = schema;
        }
    }

    public enum ADPropertyType
    {
        Ldap,
        ActiveDs
    }
}
