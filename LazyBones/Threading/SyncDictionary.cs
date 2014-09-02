using System.Collections.Generic;

namespace LazyBones.Threading
{
    //简单的线程安全的字典类
    class SyncDictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
        object syncObj = new object();

        public int Count
        {
            get { return dict.Count; }
        }

        public bool Contains(TKey key)
        {
            lock (syncObj)
            {
                return dict.ContainsKey(key);
            }
        }

        public bool Remove(TKey key)
        {
            lock (syncObj)
            {
                return dict.Remove(key);
            }
        }

        public object SyncRoot
        {
            get { return syncObj; }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (syncObj)
                {
                    return dict[key];
                }
            }
            set
            {
                lock (syncObj)
                {
                    dict[key] = value;
                }
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                lock (syncObj)
                {
                    return dict.Keys;
                }
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                lock (syncObj)
                {
                    return dict.Values;
                }
            }
        }
        public void Clear()
        {
            lock (syncObj)
            {
                dict.Clear();
            }
        }
    }
}
