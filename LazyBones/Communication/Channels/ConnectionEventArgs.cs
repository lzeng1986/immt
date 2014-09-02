using System;
using System.Net.Sockets;

namespace LazyBones.Communication.Channels
{
    public class ConnectionEventArgs : EventArgs
    {
        public Socket Socket { get; private set; }

        public object State { get; private set; }

        public ConnectionEventArgs(Socket socket)
            : this(socket, null)
        {
        }
        public ConnectionEventArgs(Socket socket, object state)
        {
            Socket = socket;
            State = state;
        }
    }
}
