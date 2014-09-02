using System;
using System.Collections.Generic;

namespace LazyBones.UI.Controls.Gantt
{
    //提供n*log(n)级别的插入删除操作，与Hashset功能相似，但内存消耗少，枚举速度快
    public class SmartList<T> : IEnumerable<T>
    {
        List<T> list = new List<T>();//该队列始终保持有序
        public bool Add(T item)
        {
            var ind = list.BinarySearch(item);
            if (ind >= 0)
                return false;
            ind = ~ind;
            list.Insert(ind, item);
            return true;
        }
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var ind = list.BinarySearch(item);
                if (ind < 0)
                {
                    ind = ~ind;
                    list.Insert(ind, item);
                }
            }
        }
        public bool Remove(T item)
        {
            var ind = list.BinarySearch(item);
            if (ind < 0)
                return false;
            list.RemoveAt(ind);
            return true;
        }
        public bool Contains(T item)
        {
            return list.BinarySearch(item) >= 0;
        }
        public int Count
        {
            get { return list.Count; }
        }
        public void Clear()
        {
            list.Clear();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
