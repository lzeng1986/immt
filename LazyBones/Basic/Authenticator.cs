using System.Security.Principal;
using LazyBones.AD;

namespace LazyBones.Basic
{
    /// <summary>
    /// 认证器，用于验证用户以及储存当前验证用户信息
    /// </summary>
    public abstract class Authenticator : IIdentity
    {
        /// <summary>
        /// 返回当前系统登录的<see cref="IIdentity"/>实例
        /// </summary>
        public abstract IIdentity Identity { get; }        
        /// <summary>
        /// 根据用户名和密码登录系统
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        public abstract OpResult Login(string userName, string password);
        /// <summary>
        /// 获取所使用的身份验证的类型。
        /// </summary>
        public string AuthenticationType
        {
            get { return Identity.AuthenticationType; }
        }
        /// <summary>
        /// 获取一个值，该值指示是否验证了用户。
        /// </summary>
        public bool IsAuthenticated
        {
            get { return Identity.IsAuthenticated; }
        }
        /// <summary>
        /// 获取当前用户的名称。
        /// </summary>
        public string Name
        {
            get { return Identity.Name; }
        }
    }
    /// <summary>
    /// 基于AD域的验证器实现
    /// </summary>
    internal class ADAuthenticator : Authenticator
    {
        public override OpResult Login(string userName, string password)
        {
            var result = ADHelper.Login(userName, password);
            switch (result)
            {
                case ADLoginResult.LOGIN_OK:
                    return new OpResult(true, "登录成功");
                case ADLoginResult.LOGIN_USER_DOESNT_EXIST:
                    return new OpResult(false, "用户不存在");
                case ADLoginResult.LOGIN_USER_LOCK:
                    return new OpResult(false, "用户已锁定");
                case ADLoginResult.LOGIN_USER_PASSWORD_INCORRECT:
                    return new OpResult(false, "密码不正确");
                case ADLoginResult.LOGIN_USER_ACCOUNT_INACTIVE:
                default:
                    return new OpResult(false, "当前用户不可用");
            }
        }

        public override IIdentity Identity
        {
            get { return WindowsIdentity.GetCurrent(); }
        }
    }
}
