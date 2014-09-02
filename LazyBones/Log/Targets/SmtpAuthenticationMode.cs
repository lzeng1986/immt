
namespace LazyBones.Log.Targets
{
    /// <summary>
    /// Smtp用户验证方式
    /// </summary>
    public enum SmtpAuthenticationMode
    {
        /// <summary>
        /// 无验证
        /// </summary>
        None,

        /// <summary>
        /// 基本验证，使用用户名和密码
        /// </summary>
        Basic,

        /// <summary>
        /// 使用Ntlm验证
        /// </summary>
        Ntlm,
    }
}
