using System;

namespace LazyBones
{
    /// <summary>
    /// 强类型延迟加载器，提供延迟加载功能，只有在使用的时候才真正创建对象
    /// <para>此类不提供构造函数，创建对象使用LazyLoader.New方法</para>
    /// <para>此类是线程安全的</para>
    /// </summary>
    public class LazyLoader<T>
    {
        bool valueCreated = false;
        T value;
        readonly Func<T> func;

        internal LazyLoader(Func<T> creator)//仅程序集内部调用，创建加载器应使用LazyLoader.New方法
        {
            func = creator;
        }

        /// <summary>
        /// 获取是否已经创建对象
        /// </summary>
        public bool ValueCreated
        {
            get { return valueCreated; }
        }
        /// <summary>
        /// 获取创建的对象
        /// </summary>
        public T Value
        {
            get
            {
                if (!valueCreated)
                {
                    lock (func) //加锁以保证对象创建时的线程安全
                    {
                        if (!valueCreated)
                        {
                            value = func();
                            valueCreated = true;
                        }
                    }
                }
                return value;
            }
        }
        /// <summary>
        /// 将<see cref="LazyLoader"/>对象转换成对应的值，当<see cref="LazyLoader"/>对象为null时，返回default(T)
        /// </summary>
        /// <param name="lazyLoader">延迟加载器</param>
        /// <returns>对应的值</returns>
        public static implicit operator T(LazyLoader<T> lazyLoader)
        {
            if (lazyLoader == null)
                return default(T);
            return lazyLoader.Value;
        }
    }
    /// <summary>
    /// 用于创建强类型延迟加载器
    /// </summary>
    public static class LazyLoader
    {
        /// <summary>
        /// 创建一个延迟加载器
        /// </summary>
        public static LazyLoader<TVal> New<TVal>()
            where TVal : new()
        {
            return new LazyLoader<TVal>(() => new TVal());
        }
        /// <summary>
        /// 创建一个延迟加载器
        /// </summary>
        /// <param name="creator">对象创建函数</param>
        public static LazyLoader<TVal> New<TVal>(Func<TVal> creator)
        {
            return new LazyLoader<TVal>(creator);
        }
    }
}
