using System;

namespace LazyBones.Basic.Data
{
    /// <summary>
    /// 表示实体映射的表
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 创建一个<see cref="TableAttribute"/>实例
        /// </summary>
        /// <param name="tableName">映射表的名称</param>
        public TableAttribute(BasicDataType dataType)
        {
            DataType = dataType;
        }
        /// <summary>
        /// 获取映射表的名称
        /// </summary>
        public BasicDataType DataType { get; private set; }
    }
}
