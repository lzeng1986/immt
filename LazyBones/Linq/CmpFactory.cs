using System;
using System.Collections.Generic;

namespace LazyBones.Linq
{
    /// <summary>
    /// 定义根据lambda表达式生成IEqualityComparer和IComparer对象的对象工厂
    /// </summary>
    [Author("曾樑")]
    public static class CmpFactory
    {
        private class EqualityComparer<TVal> : IEqualityComparer<TVal>
        {
            readonly Func<TVal, TVal, bool> op;
            public EqualityComparer(Func<TVal, TVal, bool> op)
            {
                this.op = op;
            }
            public bool Equals(TVal x, TVal y)
            {
                return op(x, y);
            }
            public int GetHashCode(TVal obj)
            {
                return 0;
            }
        }
        private class Comparer<TVal> : IComparer<TVal>
        {
            readonly Func<TVal, TVal, int> op;
            public Comparer(Func<TVal, TVal, int> op)
            {
                this.op = op;
            }
            public int Compare(TVal x, TVal y)
            {
                return op(x, y);
            }
        }     
        /// <summary>
        /// 根据lambda表达式生成对应的IEqualityComparer对象
        /// <typeparam name="T">用于比较的元素类型</typeparam>
        /// <param name="equalityCompare">判断两元素是否相等的lambda表达式</param>
        /// </summary>
        /// <returns>实现IEqualityComparer&lt;T&gt;的使用equalityCompare比较的实例</returns>
        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equalityCompare)
        {
            return new EqualityComparer<T>(equalityCompare);
        }
        /// <summary>
        /// 根据lambda表达式生成对应的IComparer对象
        /// <typeparam name="T">用于比较的元素类型</typeparam>
        /// <param name="compare">判断两元素大小的lambda表达式</param>
        /// </summary>
        /// <returns>实现IComparer&lt;T&gt;的使用compare比较的实例</returns>
        /// <remarks>lambda表达式返回值小于0表示小于，等于0表示相等，大于0表示大于</remarks>
        public static IComparer<T> Create<T>(Func<T, T, int> compare)
        {
            return new Comparer<T>(compare);
        }
    }
}
