using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyBones.Collection
{
    /// <summary>
    /// 表示强类型的循环列表
    /// </summary>
    /// <typeparam name="T">列表中元素的类型</typeparam>
    public class CircularQueue<T> : IEnumerable<T>, ICollection
    {
        int count = 0;
        int ind = 0;
        readonly int capacity;
        T[] buffer;
        public CircularQueue()
            : this(1024)
        {
        }
        public CircularQueue(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
        }
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get { return buffer[index]; }
            set { buffer[index] = value; }
        }
        public int Count { get { return count; } }
        public int Capacity { get { return capacity; } }
        public void CopyTo(Array array, int index)
        {
            var offset = ind + count - capacity;
            if (offset <= 0)
            {
                Array.Copy(buffer, ind, array, index, count);
            }
            else
            {
                var n = count - offset;
                Array.Copy(buffer, ind, array, index, n);
                Array.Copy(buffer, 0, array, index + n, offset);
            }
        }

        public bool IsSynchronized
        {
            get { return SyncRoot != null; }
        }

        public virtual object SyncRoot
        {
            get { return null; }
        }
        public void Enqueue(T item)
        {
            ind = (ind + 1) % capacity;
            buffer[ind] = item;
            count++;
            if (count > capacity)
                count = capacity;
        }
        public T Dequeue()
        {
            if (count <= 0)
                throw new InvalidOperationException("queue is empty");
            var v = buffer[ind];
            ind = (ind - 1) % capacity;
            count--;
            return v;
        }
    }
}
