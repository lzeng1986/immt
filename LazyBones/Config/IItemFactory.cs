using System;

namespace LazyBones.Config
{
    interface IItemFactory<TItemBase>
        where TItemBase : class
    {
        /// <summary>
        /// 根据配置项名称获取实例
        /// </summary>
        /// <param name="itemName">配置项名称</param>
        /// <returns>配置项实例</returns>
        TItemBase GetInstance(string itemName);
        /// <summary>
        ///  根据配置项名称获取实例类型
        /// </summary>
        /// <param name="itemName">配置项名称</param>
        /// <returns>配置项类型</returns>
        Type GetType(string itemName);

        Type[] RegisteredTypes { get; }
    }
}
