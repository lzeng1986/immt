using System.Collections.Generic;

namespace LazyBones.Data
{
    /// <summary>
    /// 一个键值为字符串的数据缓冲，此类线程安全
    /// </summary>
    public class DataMap
    {
        IDictionary<string, object> map = new Dictionary<string, object>();
        object syncLocker = new object();
        /// <summary>
        /// 添加一对值
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="obj">值</param>
        public void Add(string key, object obj)
        {
            lock (syncLocker)
            {
                map[key] = obj;
            }
        }
        public T GetValue<T>(string key)
        {
            lock (syncLocker)
            {
                return (T)map[key];
            }
        }
        public T GetValue<T>(string key, T defaultValue)
        {
            lock (syncLocker)
            {
                try
                {
                    return (T)map[key];
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        public bool TryGetValue<T>(string key, out T value)
        {
            lock (syncLocker)
            {
                value = default(T);
                object o = null;
                if (map.TryGetValue(key, out o) && (o is T))
                {
                    value = (T)o;
                    return true;
                }
                return false;
            }
        }
    }
}
