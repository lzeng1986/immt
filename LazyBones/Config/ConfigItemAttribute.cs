using System;

namespace LazyBones.Config
{
    /// <summary>
    /// 标记配置项的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class ConfigItemAttribute : Attribute
    {
        /// <summary>
        /// 获取日志项的名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 初始化 <see cref="ConfigItemAttribute"/> 类的新实例
        /// </summary>
        /// <param name="name">日志项名称</param>
        public ConfigItemAttribute(string name)
        {
            Name = name;
        }
    }
}
