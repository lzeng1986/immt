using System.Net;
using System.Net.Security;
using System.Security.Principal;

namespace LazyBones.Communication.Security
{
    class NegotiateBinding : ISecurityBinding
    {
        public NegotiateBinding()
        {
            Credential = CredentialCache.DefaultNetworkCredentials;
            RequiredProtectionLevel = ProtectionLevel.None;
            AllowedImpersonationLevel = TokenImpersonationLevel.Identification;
        }
        public CredentialType CredentialType
        {
            get { return CredentialType.Windows; }
        }
        public NetworkCredential Credential { get; set; }
        public ProtectionLevel RequiredProtectionLevel { get; set; }
        public TokenImpersonationLevel AllowedImpersonationLevel { get; set; }
    }
}
