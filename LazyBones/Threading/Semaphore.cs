using System;
using System.Threading;

namespace LazyBones.Threading
{
    class Semaphore
    {
        private int count;
        public Semaphore()
            : this(1)
        {
        }
        public Semaphore(int count)
        {
            if (count < 0) 
                throw new ArgumentException("信号量不得小于0", "count");
            this.count = count;
        }
        public void AddOne()
        {
            lock (this)
            {
                count++;
                Monitor.Pulse(this);
            }
        }
        public void WaitOne()
        {
            lock (this)
            {
                Monitor.Wait(this);
                count--;
            }
        }
        public bool WaitOne(int millisecondsTimeout)
        {
            lock (this)
            {
                var result = false;
                result = Monitor.Wait(this, millisecondsTimeout);
                if(result)
                    count--;
                return result;
            }
        }
        public void Reset(int count)
        {
            lock (this)
            {
                this.count = count;
            }
        }
    }
}
