using System;

namespace LazyBones.UI
{
    /// <summary>
    /// 指定菜单项的Id
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MenuItemIdAttribute : Attribute
    {
        /// <summary>
        /// 创建一个<see cref="MenuItemIdAttribute"/>实例
        /// </summary>
        /// <param name="menuName">菜单名称</param>
        public MenuItemIdAttribute(string menuName)
        {
            MenuName = menuName;
        }
        /// <summary>
        /// 获取菜单的名称
        /// </summary>
        public string MenuName { get; private set; }
    }
}
