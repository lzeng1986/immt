using System.Security.Principal;
using System.Text;
using LazyBones.AD;
using LazyBones.Log.Config;
using System;
using System.ComponentModel;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 对象<see cref="LogEvent"/>域账号信息渲染器
    /// </summary>
    [Renderer("ad")]
    public class ADRenderer : Renderer
    {
        [DefaultValue(true)]
        public bool IncludeState { get; set; }

        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            var userName = Environment.UserName;
            var adUser = ADHelper.GetUser(userName).ToADUser();
            sb.Append(adUser.UserName);
            sb.Append('-');
            sb.Append(adUser.DisplayName);
            sb.Append('-');
            if (IncludeState)
            {
                if (!adUser.Actived)
                {
                    sb.Append("未激活");
                    sb.Append('-');
                }
                if (adUser.Locked)
                {
                    sb.Append("账号锁定");
                    sb.Append('-');
                }
            }
            sb.Append("上次设置密码时间：" + adUser.PasswordLastSet);
        }
    }
}
