using System.Collections.Generic;

namespace LazyBones.Basic.Data
{
    /// <summary>
    /// 数据源接口，用于<see cref="Framework"/>
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// 加载特定类型的数据
        /// </summary>
        /// <param name="dataType">加载数据的类型</param>
        /// <param name="dataCollection">填充的数据集合</param>
        void LoadData(BasicDataType dataType, List<Entity> dataCollection);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <typeparam name="T">创建对象类型</typeparam>
        /// <param name="dataType">创建对象类型</param>
        /// <param name="entity">创建对象</param>
        void Create<T>(BasicDataType dataType, T entity) where T : Entity;
        /// <summary>
        /// 更新一个实体
        /// </summary>
        /// <typeparam name="T">更新对象类型</typeparam>
        /// <param name="dataType">更新对象类型</param>
        /// <param name="entity">更新对象</param>
        void Update<T>(BasicDataType dataType, T entity) where T : Entity;
        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <typeparam name="T">删除对象类型</typeparam>
        /// <param name="dataType">删除对象类型</param>
        /// <param name="entity">删除对象</param>
        void Delete<T>(BasicDataType dataType, T entity) where T : Entity;
    }
}
