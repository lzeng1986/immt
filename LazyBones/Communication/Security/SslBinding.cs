using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace LazyBones.Communication.Security
{
    class SslBinding : ISecurityBinding
    {
        public SslBinding()
        {
            SslProtocol = SslProtocols.Default;
        }
        public CredentialType CredentialType
        {
            get { return CredentialType.Certificate; }
        }
        public SslProtocols SslProtocol { get; set; }
        public X509Certificate Certificate { get; set; }
        public string TargetHost { get; set; }
    }
}
