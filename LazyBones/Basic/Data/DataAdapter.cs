using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Extensions;

namespace LazyBones.Basic.Data
{
    //数据源适配器
    class DataAdapter
    {
        //数据的本地缓存
        Dictionary<BasicDataType, List<Entity>> dataSet = new Dictionary<BasicDataType, List<Entity>>();
        IDataSource dataSource;
        public DataAdapter(IDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        Dictionary<Type, BasicDataType> tableAttributeCache = new Dictionary<Type, BasicDataType>();

        BasicDataType GetTableName<T>()
        {
            BasicDataType dataType = BasicDataType.Undefined;
            if (!tableAttributeCache.TryGetValue(typeof(T), out dataType))
            {
                var attr = typeof(T).GetSingleAttribute<TableAttribute>(false);
                if (attr != null)
                {
                    tableAttributeCache.Add(typeof(T), attr.DataType);
                    dataType = attr.DataType;
                }
            }
            return dataType;
        }
        void CheckTableName(BasicDataType dataType)
        {
            if (dataSet.ContainsKey(dataType))
                return;
            throw new InvalidOperationException("没有针对<" + dataType + ">的数据填充");
        }
        /// <summary>
        /// 更新一个对象
        /// </summary>
        /// <typeparam name="T">更新对象类型</typeparam>
        /// <param name="item">更新的对象</param>
        public void Update<T>(T item)
            where T : Entity
        {
            var dataType = GetTableName<T>();
            if (dataType == BasicDataType.Undefined)
                return;
            CheckTableName(dataType);
            var ind = dataSet[dataType].FindIndex(o => EqualityComparer<T>.Default.Equals(o as T, item));
            if (ind < 0)
                throw new ArgumentException("不存在指定对象，无法进行更新");
            dataSource.Update(dataType, item);
        }
        /// <summary>
        /// 删除一个对象
        /// </summary>
        /// <typeparam name="T">删除对象类型</typeparam>
        /// <param name="item">删除的对象</param>
        public void Remove<T>(T item)
            where T : Entity
        {
            var dataType = GetTableName<T>();
            if (dataType == BasicDataType.Undefined)
                return;
            CheckTableName(dataType);
            if (dataSet[dataType].Remove(item))
            {
                item.IsDeleted = true;
                dataSource.Delete(dataType, item);
            }
        }
        /// <summary>
        /// 删除满足条件的对象
        /// </summary>
        /// <typeparam name="T">删除对象类型</typeparam>
        /// <param name="predicate">删除条件</param>
        public void RemoveAll<T>(Func<T, bool> predicate)
            where T : Entity
        {
            var dataType = GetTableName<T>();
            if (dataType == BasicDataType.Undefined)
                return;
            CheckTableName(dataType);
            var list = new List<Entity>();
            foreach (var d in dataSet[dataType])
            {
                if (predicate(d as T))
                {
                    d.IsDeleted = true;
                    dataSource.Delete(dataType, d);
                }
                else
                    list.Add(d);
            }
            dataSet[dataType] = list;
            OnDataRefreshed();
        }
        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <typeparam name="T">创建对象类型</typeparam>
        /// <param name="item">创建对象</param>
        public void Add<T>(T item)
            where T : Entity
        {
            var dataType = GetTableName<T>();
            if (dataType == BasicDataType.Undefined)
                return;
            CheckTableName(dataType);
            var ind = dataSet[dataType].FindIndex(o => EqualityComparer<T>.Default.Equals(o as T, item));
            if (ind >= 0)
                throw new ArgumentException("需要创建的对象已存在");
            dataSource.Create(dataType, item);
            dataSet[dataType].Add(item);
        }
        /// <summary>
        /// 获取指定类型的所有对象实例
        /// </summary>
        /// <typeparam name="T">获取实例的类型</typeparam>
        /// <returns>实例列表</returns>
        public T[] GetAll<T>()
            where T : Entity
        {
            var dataType = GetTableName<T>();
            if (dataType == BasicDataType.Undefined)
                return new T[0];
            if (!dataSet.ContainsKey(dataType))
            {
                dataSet.Add(dataType, new List<Entity>());
                dataSet[dataType].Sort();
                dataSource.LoadData(dataType, dataSet[dataType]);
            }
            return dataSet[dataType].Cast<T>().ToArray();
        }
        /// <summary>
        /// 刷新数据源
        /// </summary>
        public void Refresh()
        {
            foreach (var item in dataSet)
            {
                item.Value.Clear();
                dataSource.LoadData(item.Key, item.Value);
            }
            OnDataRefreshed();
        }
        /// <summary>
        /// 触发DataRefreshed事件
        /// </summary>
        protected void OnDataRefreshed()
        {
            if (DataRefreshed != null)
                DataRefreshed(this, EventArgs.Empty);
        }
        /// <summary>
        /// 当数据源刷新时发生
        /// </summary>
        public event EventHandler DataRefreshed;
    }
}
