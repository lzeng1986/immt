using System;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 在加载日志配置文件或者使用配置时引发的异常
    /// </summary>
    public class LogConfigException : Exception
    {
        /// <summary>
        /// 用给定异常消息初始化<see cref="LogConfigException"/>实例
        /// </summary>
        /// <param name="message">异常消息</param>
        public LogConfigException(string message)
            : base(message)
        {
        }
        public LogConfigException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        internal LogConfigException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
        internal LogConfigException(Exception innerException, string format, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }
    }
}
