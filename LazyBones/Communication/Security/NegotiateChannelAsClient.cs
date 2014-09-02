using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using LazyBones.Communication.Channels;
using LazyBones.Log;

namespace LazyBones.Communication.Security
{
    class NegotiateChannelAsClient : StreamChannel
    {
        static Logger logger = LogManager.Current;
        string serverName;
        public NetworkCredential Credential { get; set; }
        public ProtectionLevel RequiredProtectionLevel { get; set; }
        public TokenImpersonationLevel AllowedImpersonationLevel { get; set; }
        public NegotiateChannelAsClient(Socket clientSocket, string serverName)
            : base(clientSocket)
        {
            this.serverName = serverName;
            Credential = CredentialCache.DefaultNetworkCredentials;
            RequiredProtectionLevel = ProtectionLevel.None;
            AllowedImpersonationLevel = TokenImpersonationLevel.Identification;
        }
        protected override void OpenCommunicator()
        {
            try
            {
                var negotiateStream = new NegotiateStream(new NetworkStream(clientSocket), false);
                negotiateStream.BeginAuthenticateAsClient(Credential, serverName, RequiredProtectionLevel, AllowedImpersonationLevel, EndAuthenticate, negotiateStream);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                OnError(e);
            }
        }
        void EndAuthenticate(IAsyncResult result)
        {
            var negotiateStream = result.AsyncState as NegotiateStream;
            try
            {
                negotiateStream.EndAuthenticateAsServer(result);
                stream = negotiateStream;
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
