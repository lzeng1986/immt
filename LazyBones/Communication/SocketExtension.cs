using System.Net.Sockets;

namespace LazyBones.Communication
{
    static class SocketExtension
    {
        public static void SafeClose(this Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            finally
            {
                socket.Close();
            }
        }
    }
}
