using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LazyBones.Communication
{
    /// <summary>
    /// 用于<see cref="SocketAsyncEventArgs"/>缓冲区管理，提供设置和回收机制，以提高性能
    /// </summary>
    public class BufferManager : IDisposable
    {
        readonly int bufferTotalSize;
        readonly int bufferSize;
        byte[] largeBuffer;
        int currentIndex = 0;
        Stack<int> offsets;
        bool isDisposed = false;
        /// <summary>
        /// 创建一个新的<see cref="BufferManager"/>对象
        /// </summary>
        /// <param name="bufferNum">可管理的缓冲区最大数量</param>
        /// <param name="bufferSize">每个缓冲区大小</param>
        public BufferManager(int bufferNum, int bufferSize)
        {
            if (bufferNum <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferNum", "参数值必须大于零");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", "参数值必须大于零");
            }
            this.bufferTotalSize = bufferNum * bufferSize;
            this.bufferSize = bufferSize;
            offsets = new Stack<int>(bufferNum);
            largeBuffer = new byte[bufferNum * bufferSize];
        }
        /// <summary>
        /// 释放所有资源，释放后的对象不可使用
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            largeBuffer = null;
            offsets.Clear();
            offsets = null;
        }
        void CheckDispose()
        {
            if (isDisposed)
            {
                throw new InvalidOperationException("对象已被释放");
            }
        }
        public bool TryGet(out ArraySegment<byte> buffer)
        {
            CheckDispose();
            if (offsets.Count > 0)
            {
                buffer = new ArraySegment<byte>(largeBuffer, offsets.Pop(), bufferSize);
                return true;
            }
            else if ((currentIndex + bufferSize) > bufferTotalSize)
            {
                buffer = default(ArraySegment<byte>);
                return false;
            }
            else
            {
                buffer = new ArraySegment<byte>(largeBuffer, currentIndex, bufferSize);
                currentIndex += bufferSize;
                return true;
            }
        }
        public void Release(ArraySegment<byte> buffer)
        {
            CheckDispose();
            offsets.Push(buffer.Offset);
        }
    }
}
