using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.Communication.Core
{
    internal static class Helper
    {
        internal static bool IsSuccess(this SocketAsyncEventArgs e)
        {
            switch (e.SocketError)
            {
                case SocketError.Success:
                //socket已经关闭
                case SocketError.OperationAborted:
                case SocketError.Interrupted:
                case SocketError.NotSocket:
                    return true;
                default:
                    return false;
            }
        }
        internal static void SafeClose(this Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket.Close();
            }
        }
        internal static void Send(this Socket socket, byte[] data, int offset, int count)
        {
            while (count > 0)
            {
                var size = socket.Send(data, offset, count, SocketFlags.None);
                count -= size;
                offset += size;
            }
        }

        internal static int IndexOf<T>(this IEnumerable<T> source, int startIndex, IEnumerable<T> value)
            where T : IEquatable<T>
        {
            var ind = 0;
            var souceE = source.Skip(startIndex).GetEnumerator();
            var valueE = value.GetEnumerator();
            if (!valueE.MoveNext())
                throw new ArgumentNullException("value");
            while (souceE.MoveNext())
            {
                if (souceE.Current.Equals(valueE.Current))
                {
                    if (!valueE.MoveNext())
                        return ind - value.Count();
                }
                else
                {
                    valueE.Reset();
                    valueE.MoveNext();
                }
                ind++;
            }
            return -1;
        }
    }
}
