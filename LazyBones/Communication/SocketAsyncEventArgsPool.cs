using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace LazyBones.Communication
{
    class SocketAsyncEventArgsPool : IDisposable
    {
        BufferManager bufferManager;
        volatile bool isDisposed = false;
        Stack<SocketAsyncEventArgs> pool;
        public SocketAsyncEventArgsPool(int maxNum, int bufferSize)
        {
            bufferManager = new BufferManager(maxNum, bufferSize);
            pool = new Stack<SocketAsyncEventArgs>(maxNum);
        }
        public SocketAsyncEventArgs GetOne()
        {
            lock (pool)
            {
                ArraySegment<byte> buffer;
                if (pool.Count > 0 && bufferManager.TryGet(out buffer))
                {
                    var args = pool.Pop();
                    args.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
                    return args;
                }
                return null;
            }
        }
        public void Release(SocketAsyncEventArgs args)
        {
            lock (pool)
            {
                bufferManager.Release(new ArraySegment<byte>(args.Buffer,args.Offset,args.Count));
                pool.Push(args);
            }
        }
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            bufferManager.Dispose();
            bufferManager = null;
            foreach (var v in pool)
            {
                v.Dispose();
            }
            pool.Clear();
            pool = null;
            GC.SuppressFinalize(true);
        }
    }
}
