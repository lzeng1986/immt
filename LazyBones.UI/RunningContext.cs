using System;
using LazyBones.Basic;

namespace LazyBones.UI
{
    /// <summary>
    /// 应用系统运行上下文
    /// </summary>
    public abstract class RunningContext
    {
        /// <summary>
        /// 获取应用程序当前用户
        /// </summary>
        public abstract User CurrentUser { get; }
        /// <summary>
        /// 获取应用系统名称
        /// </summary>
        public abstract string AppName { get; }
        /// <summary>
        /// 获取应用程序版本
        /// </summary>
        public abstract Version AppVersion { get; }
    }
}
