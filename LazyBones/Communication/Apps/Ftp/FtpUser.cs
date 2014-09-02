using System.Linq;
using System.Net;
using LazyBones.Extensions;
using LazyBones.Log;

namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpUser
    {
        static Logger logger = LogManager.Current;
        string password;
        IPAddress[] allowedIP;
        IPAddress[] refusedIP;
        UserPermissions permissions;
        public string Root { get; private set; }
        public string User { get; private set; }

        public FtpUser(FtpPolicy policy)
        {
            User = policy.User;
            password = policy.Password;
            allowedIP = GetIPFromString(policy.AllowedIP);
            refusedIP = GetIPFromString(policy.RefusedIP);
            Root = policy.Root;
            permissions = policy.Permission;
        }

        static IPAddress[] GetIPFromString(string ipStr)
        {
            if (string.IsNullOrEmpty(ipStr))
                return null;
            return ipStr.Split(';').Where(s => s != null && s.Trim().Length != 0).Select(s => IPAddress.Parse(s)).ToArray();
        }
        
        public bool Authenticate(string password, IPAddress userIP)
        {
            if (!this.password.Equals(password))
            {
                logger.Warn("用户'{0}'登录失败,输入密码错误.from:{1}", User, userIP);
                return false;
            }
            if (refusedIP != null && refusedIP.Any(ip => ip.Equals(userIP)))
            {
                logger.Warn("用户'{0}'登录失败,用户ip在拒绝访问列表中.from:{1}", User, userIP);
                return false;
            }
            if (allowedIP != null && !allowedIP.Any(ip => ip.Equals(userIP)))
            {
                logger.Warn("用户'{0}'登录失败,用户ip不在允许访问列表中.from:{1}", User, userIP);
                return false;
            }
            logger.Info("用户'{0}'登录成功.from:{1}", User, userIP);
            return true;
        }

        public bool HasPermission(UserPermissions permission)
        {
            return permissions.HasFlag(permission);
        }
    }
}
