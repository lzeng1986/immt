using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using LazyBones.Communication.Channels;
using LazyBones.Log;
using System.Net.Security;

namespace LazyBones.Communication.Security
{
    class SslChannelAsServer : StreamChannel
    {
        static Logger logger = LogManager.Current;
        SslProtocols SslProtocol { get; set; }
        public X509Certificate Certificate { get; private set; }
        public SslChannelAsServer(Socket clientSocket, X509Certificate certificate)
            : base(clientSocket)
        {
            SslProtocol = SslProtocols.Default;
            Certificate = certificate;
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
                        sslStream.BeginAuthenticateAsServer(Certificate, false, SslProtocols.Default, false, EndAuthenticate, sslStream);
                        break;
                    case (SslProtocols.Ssl2):
                        var ssl2Stream = new SslStream(new NetworkStream(clientSocket), false);
                        ssl2Stream.BeginAuthenticateAsServer(Certificate, false, SslProtocols.Ssl2, false, EndAuthenticate, ssl2Stream);
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
