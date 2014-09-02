using System;

namespace LazyBones.Basic.Data
{
    /// <summary>
    /// 数据对象的抽象基类
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// 获取该实体是否新创建
        /// </summary>
        public bool IsNew { get; internal set; }
        /// <summary>
        /// 获取该实体是否被删除
        /// </summary>
        public bool IsDeleted { get; internal set; }
    }
}
