using System.Collections.Generic;

namespace LazyBones.Collection
{
    /// <summary>
    /// 双向迭代器
    /// </summary>
    /// <typeparam name="T">迭代器值的类型</typeparam>
    public interface IDuplexEnumerator<T> : IEnumerator<T>
    {
        bool MovePrev();
    }
}
