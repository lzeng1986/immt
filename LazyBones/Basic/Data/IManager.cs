using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Basic.Data
{
    /// <summary>
    /// 对象管理器的接口
    /// </summary>
    /// <typeparam name="T">管理对象的类型</typeparam>
    public interface IManager<T> : IEnumerable<T>, IDisposable
        where T : Entity
    {
        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <param name="item">创建的对象</param>
        void Add(T item);
        /// <summary>
        /// 更新一个对象
        /// </summary>
        /// <param name="item">更新的对象</param>
        void Update(T item);
        /// <summary>
        /// 删除一个对象
        /// </summary>
        /// <param name="item">删除的对象</param>
        void Remove(T item);
        /// <summary>
        /// 列出所有对象
        /// </summary>
        /// <returns>对象列表</returns>
        T[] List();
        /// <summary>
        /// 判断管理器是否包含指定对象
        /// </summary>
        /// <param name="item">检查的对象</param>
        /// <returns>是否包含</returns>
        bool Contains(T item);
    }

    /// <summary>
    /// 对象管理器
    /// </summary>
    /// <typeparam name="T">管理对象的类型</typeparam>
    public class Manager<T> : IManager<T>
        where T : Entity
    {
        DataAdapter dataProvider;
        bool isDataDirty = true;    //指示数据是否应该重新加载，初始值为true，保证第一次的数据加载

        internal Manager(DataAdapter dataProvider)
        {
            this.dataProvider = dataProvider;
            dataProvider.DataRefreshed += dataProvider_DataRefreshed;
        }
        void dataProvider_DataRefreshed(object sender, System.EventArgs e)
        {
            isDataDirty = true;
        }
        public virtual void Add(T item)
        {
            dataProvider.Add(item);
            isDataDirty = true;
        }
        public virtual void Update(T item)
        {
            dataProvider.Update(item);
        }

        public virtual void Remove(T item)
        {
            dataProvider.Remove(item);
            isDataDirty = true;
        }
        T[] dataCache;
        public virtual T[] List()
        {
            if (isDataDirty)
            {
                dataCache = null;
                dataCache = dataProvider.GetAll<T>();
                isDataDirty = false;
            }
            return dataCache;
        }

        public virtual bool Contains(T item)
        {
            return List().Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List().AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return List().GetEnumerator();
        }

        public void Dispose()
        {
            dataProvider.DataRefreshed -= dataProvider_DataRefreshed;
        }
    }
}
