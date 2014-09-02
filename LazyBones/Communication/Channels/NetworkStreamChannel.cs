using System.Net.Sockets;

namespace LazyBones.Communication.Channels
{
    class NetworkStreamChannel : StreamChannel
    {
        public NetworkStreamChannel(Socket socket)
            : base(socket)
        {
        }
        protected override void OpenCommunicator()
        {
            stream = new NetworkStream(clientSocket);
            Receive();
        }
    }
}
