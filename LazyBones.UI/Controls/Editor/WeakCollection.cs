using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.UI.Controls.Editor
{
    class WeakCollection<T> : IEnumerable<T>
        where T : class
    {
        readonly List<WeakReference> innerList = new List<WeakReference>();

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            CheckNoEnumerator();
            if (innerList.Count == innerList.Capacity || (innerList.Count % 32) == 31)
                innerList.RemoveAll(r => !r.IsAlive);
            innerList.Add(new WeakReference(item));
        }

        /// <summary>
        /// Removes all elements from the collection. Runtime: O(n).
        /// </summary>
        public void Clear()
        {
            innerList.Clear();
            CheckNoEnumerator();
        }

        /// <summary>
        /// Checks if the collection contains an item. Runtime: O(n).
        /// </summary>
        public bool Contains(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            CheckNoEnumerator();
            return this.Any(t => t == item);
        }

        public bool Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            CheckNoEnumerator();
            for (int i = 0; i < innerList.Count; )
            {
                T element = (T)innerList[i].Target;
                if (element == null)
                {
                    RemoveAt(i);
                }
                else if (element == item)
                {
                    RemoveAt(i);
                    return true;
                }
                else
                {
                    i++;
                }
            }
            return false;
        }

        void RemoveAt(int i)
        {
            var lastIndex = innerList.Count - 1;
            innerList[i] = innerList[lastIndex];
            innerList.RemoveAt(lastIndex);
        }

        bool hasEnumerator;

        void CheckNoEnumerator()
        {
            if (hasEnumerator)
                throw new InvalidOperationException("The WeakCollection is already being enumerated, it cannot be modified at the same time. Ensure you dispose the first enumerator before modifying the WeakCollection.");
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (hasEnumerator)
                throw new InvalidOperationException("The WeakCollection is already being enumerated, it cannot be enumerated twice at the same time. Ensure you dispose the first enumerator before using another enumerator.");
            try
            {
                hasEnumerator = true;
                for (int i = 0; i < innerList.Count; )
                {
                    T element = (T)innerList[i].Target;
                    if (element == null)
                    {
                        RemoveAt(i);
                    }
                    else
                    {
                        yield return element;
                        i++;
                    }
                }
            }
            finally
            {
                hasEnumerator = false;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
