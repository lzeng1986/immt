using System;
using System.Collections;

namespace LazyBones.Utils
{
    /// <summary>
    /// 辅助类，主要用于检查对象是否满足一定的条件
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// 检查集合是否为空
        /// </summary>
        /// <param name="collection">检查的集合</param>
        /// <returns>集合等于null或为空，则返回<see lang="true"/></returns>
        public static bool IsEmpty(this IEnumerable collection)
        {
            if (collection == null)
                return true;
            return !collection.GetEnumerator().MoveNext();
        }
        /// <summary>
        /// 检查集合是否非空
        /// </summary>
        /// <param name="collection">检查的集合</param>
        /// <returns>集合非空且有值，则返回<see lang="true"/></returns>
        public static bool HasValue(this IEnumerable collection)
        {
            if (collection == null)
                return false;
            return collection.GetEnumerator().MoveNext();
        }
    }
}
