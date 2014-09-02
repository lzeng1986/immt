using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using LazyBones.Communication.Channels;
using LazyBones.Log;

namespace LazyBones.Communication.Security
{
    class SslChannelAsClient : StreamChannel
    {
        static Logger logger = LogManager.Current;
        public SslProtocols SslProtocol { get; set; }
        public X509Certificate Certificate { get; private set; }
        string targetHost;
        public SslChannelAsClient(Socket clientSocket, X509Certificate certificate, string targetHost)
            : base(clientSocket)
        {
            SslProtocol = SslProtocols.Default;
            Certificate = certificate;
            this.targetHost = targetHost;
        }
        protected override void OpenCommunicator()
        {
            try
            {
                switch (SslProtocol)
                {
                    case (SslProtocols.Default):
                    case (SslProtocols.Tls):
                    case (SslProtocols.Ssl3):
                        var sslStream = new SslStream(new NetworkStream(clientSocket), false);
                        sslStream.BeginAuthenticateAsClient(targetHost, EndAuthenticate, sslStream);
                        break;
                    case (SslProtocols.Ssl2):
                        var ssl2Stream = new SslStream(new NetworkStream(clientSocket), false);
                        ssl2Stream.BeginAuthenticateAsClient(targetHost, EndAuthenticate, ssl2Stream);
                        break;
                    default:
                        stream = new NetworkStream(clientSocket);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                OnError(e);
            }
        }
        void EndAuthenticate(IAsyncResult result)
        {
            var sslStream = result.AsyncState as SslStream;
            try
            {
                sslStream.EndAuthenticateAsServer(result);
                stream = sslStream;
                Receive();
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                OnError(e);
            }
        }
    }
}
