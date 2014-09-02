using System;

namespace LazyBones.Config
{
    public class ConfigException : Exception
    {
        /// <summary>
        /// 用给定异常消息初始化<see cref="ConfigException"/>实例
        /// </summary>
        /// <param name="message">异常消息</param>
        public ConfigException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// 用给定格式化异常消息初始化<see cref="ConfigException"/>实例
        /// </summary>
        /// <param name="format">消息格式</param>
        /// <param name="args">消息参数</param>
        public ConfigException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}
