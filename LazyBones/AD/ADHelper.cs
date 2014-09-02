using System;
using System.Text;
using System.DirectoryServices;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;

namespace LazyBones.AD
{
    /// <summary>
    /// 对AD操作的封装，对于AD操作的说明，在doc文件夹下参考文档中有详细叙述
    /// </summary>
    public static class ADHelper
    {
        readonly static string ADPath;
        static ADHelper()
        {
            ADPath = ConfigurationManager.AppSettings["ADPath"];
            if (string.IsNullOrEmpty(ADPath))
            {
                ADPath = "LDAP://" + Domain.GetCurrentDomain().Name;
            }
        }
        static DirectoryEntry GetDirectoryObject()
        {
            return new DirectoryEntry(ADPath);
        }
        static DirectoryEntry GetDirectoryObject(string UserName, string Password)
        {
            return new DirectoryEntry(ADPath, UserName, Password, AuthenticationTypes.Secure);
        }
        public static ADUser GetADUser(string userName)
        {
            return GetUser(userName).ToADUser();
        }
        public static DirectoryEntry GetUser(string userName)
        {
            DirectorySearcher deSearch = new DirectorySearcher();
            deSearch.SearchRoot = new DirectoryEntry(ADPath);
            deSearch.Filter = "(&(objectClass=user)(sAMAccountName=" + userName + "))";
            deSearch.SearchScope = SearchScope.Subtree;
            try
            {
                var result = deSearch.FindOne();
                if (result == null)
                    return null;
                return new DirectoryEntry(result.Path);
            }
            catch
            {
                return null;
            }
        }
        public static DirectoryEntry GetUser(string userName, string password)
        {
            DirectorySearcher deSearch = new DirectorySearcher();
            deSearch.SearchRoot = new DirectoryEntry(ADPath, userName, password, AuthenticationTypes.Secure);
            deSearch.Filter = "(&(objectClass=user)(sAMAccountName=" + userName + "))";
            deSearch.SearchScope = SearchScope.Subtree;
            try
            {
                var result = deSearch.FindOne();
                if (result == null)
                    return null;
                return new DirectoryEntry(result.Path);
            }
            catch
            {
                return null;
            }
        }
        public static void SetPassword(this DirectoryEntry de, string password)
        {
            de.Invoke("SetPassword", new object[] { password });
        }
        internal static string GetProperty(this DirectoryEntry de, string propertyName)
        {
            if (de.Properties.Contains(propertyName))
            {
                return de.Properties[propertyName][0].ToString();
            }
            return string.Empty;
        }
        static bool IsAccountActive(this DirectoryEntry de)
        {
            var flag = Convert.ToInt32(de.GetProperty("userAccountControl"));
            return (flag & (int)ADAccountFlags.UF_ACCOUNTDISABLE) == 0;
        }
        static bool IsAccountLock(this DirectoryEntry de)
        {
            return Convert.ToBoolean(de.InvokeGet("IsAccountLocked"));
        }
        internal static string GetLDAPDomain()
        {
            var LDAPDomain = new StringBuilder();
            string serverName = "k2mega.local";
            string[] LDAPDC = serverName.Split(System.Convert.ToChar("."));
            int i = 0;
            while (i < LDAPDC.GetUpperBound(0) + 1)
            {
                LDAPDomain.Append("DC=" + LDAPDC[i]);
                if (i < LDAPDC.GetUpperBound(0))
                {
                    LDAPDomain.Append(",");
                }
                i += 1;
            }
            return LDAPDomain.ToString();
        }
        internal static DirectoryEntry GetDirectoryObjectByDistinguishedName(string ObjectPath)
        {
            DirectoryEntry oDE;
            oDE = new DirectoryEntry(ObjectPath);
            return oDE;
        }
        public static ADUser ToADUser(this DirectoryEntry de)
        {
            var ADUser = new ADUser();

            ADUser.DisplayName = de.GetProperty("displayName");
            ADUser.Department = de.GetProperty("department");
            ADUser.UserName = de.GetProperty("sAMAccountName");
            ADUser.UserPrincipalName = de.GetProperty("UserPrincipalName");
            ADUser.Email = de.GetProperty("mail");
            ADUser.MailNickname = de.GetProperty("mailNickname");
            ADUser.AccountControl = (ADAccountFlags)Convert.ToInt32(de.GetProperty("userAccountControl"));
            ADUser.Actived = de.IsAccountActive();
            ADUser.Locked = de.IsAccountLock();
            ADUser.PasswordLastSet = Convert.ToDateTime(de.InvokeGet("PasswordLastChanged"));
            ADUser.AccountExpirationDate = Convert.ToDateTime(de.InvokeGet("AccountExpirationDate"));
            return ADUser;
        }
        public static ADLoginResult Login(string userName, string password)
        {
            var de = GetUser(userName);
            if (de == null)
                return ADLoginResult.LOGIN_USER_DOESNT_EXIST;
            using (de)
            {
                if (!de.IsAccountActive())
                    return ADLoginResult.LOGIN_USER_ACCOUNT_INACTIVE;
                if (de.IsAccountLock())
                    return ADLoginResult.LOGIN_USER_LOCK;
                if (GetUser(userName, password) == null)
                    return ADLoginResult.LOGIN_USER_PASSWORD_INCORRECT;
                else
                    return ADLoginResult.LOGIN_OK;
            }
        }
    }
}
