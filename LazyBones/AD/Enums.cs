using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.AD
{
    /// <summary>
    /// 登陆结果
    /// </summary>
    public enum ADLoginResult
    {
        /// <summary>
        /// 登陆成功
        /// </summary>
        LOGIN_OK = 0,
        /// <summary>
        /// 用户不存在
        /// </summary>
        LOGIN_USER_DOESNT_EXIST,
        /// <summary>
        /// 用户账号被禁用
        /// </summary>
        LOGIN_USER_ACCOUNT_INACTIVE,
        /// <summary>
        /// 用户密码不正确
        /// </summary>
        LOGIN_USER_PASSWORD_INCORRECT,
        /// <summary>
        /// 用户被锁定
        /// </summary>
        LOGIN_USER_LOCK
    }
    public enum UserStatus
    {
        Enable = 544,
        Disable = 546
    }
    public enum GroupScope
    {
        ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP = -2147483644,
        ADS_GROUP_TYPE_GLOBAL_GROUP = -2147483646,
        ADS_GROUP_TYPE_UNIVERSAL_GROUP = -2147483640
    }
    /// <summary>
    /// 用户属性标志
    /// </summary>
    [Flags]
    public enum ADAccountFlags
    {
        /// <summary>
        /// 登录脚本标志。如果通过ADSI LDAP进行读写操作，则该标志失效；如果通过ADSI WINNT，则该标志为只读。
        /// </summary>
        UF_SCRIPT = 0x1,
        /// <summary>
        /// 账号禁用标志
        /// </summary>
        UF_ACCOUNTDISABLE = 0x2,
        /// <summary>
        /// 主文件夹标志
        /// </summary>
        UF_HOMEDIR_REQUIRED = 0x8,
        /// <summary>
        /// 过期标志
        /// </summary>
        UF_LOCKOUT = 0x10,
        /// <summary>
        /// 密码不是必须的
        /// </summary>
        UF_PASSWD_NOTREQD = 0x20,
        /// <summary>
        /// 用户不能更改密码
        /// </summary>
        UF_PASSWD_CANT_CHANGE = 0x40,
        /// <summary>
        /// 使用可逆加密算法保存密码
        /// </summary>
        UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0x80,
        /// <summary>
        /// 本地账号
        /// </summary>
        UF_TEMP_DUPLICATE_ACCOUNT = 0x100,
        /// <summary>
        /// 普通用户
        /// </summary>
        UF_NORMAL_ACCOUNT = 0x200,
        /// <summary>
        /// 跨域信任账号
        /// </summary>
        UF_INTERDOMAIN_TRUST_ACCOUNT = 0x800,
        /// <summary>
        /// 工作站信任账号
        /// </summary>
        UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,
        /// <summary>
        /// 服务器信任账号
        /// </summary>
        UF_SERVER_TRUST_ACCOUNT = 0x2000,
        /// <summary>
        /// 密码永不过期
        /// </summary>
        UF_DONT_EXPIRE_PASSWD = 0x10000
    }
}
