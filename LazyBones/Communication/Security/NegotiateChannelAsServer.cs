using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using LazyBones.Communication.Channels;
using LazyBones.Log;

namespace LazyBones.Communication.Security
{
    class NegotiateChannelAsServer : StreamChannel
    {
        static Logger logger = LogManager.Current;
        public NetworkCredential Credential { get; set; }
        public ProtectionLevel RequiredProtectionLevel { get; set; }
        public TokenImpersonationLevel AllowedImpersonationLevel { get; set; }
        public IIdentity RemoteIdentity { get; private set; }

        public NegotiateChannelAsServer(Socket clientSocket)
            : base(clientSocket)
        {
            Credential = CredentialCache.DefaultNetworkCredentials;
            RequiredProtectionLevel = ProtectionLevel.None;
            AllowedImpersonationLevel = TokenImpersonationLevel.Identification;
        }
        protected override void OpenCommunicator()
        {
            try
            {
                var negotiateStream = new NegotiateStream(new NetworkStream(clientSocket), false);
                negotiateStream.BeginAuthenticateAsServer(Credential, RequiredProtectionLevel, AllowedImpersonationLevel, EndAuthenticate, negotiateStream);
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
                RemoteIdentity = negotiateStream.RemoteIdentity;
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
