using System;

namespace LazyBones.Communication.Security
{
    public static class SecurityBindingFactory
    {
        public static ISecurityBinding CreateSecurityBinding(CredentialType credentialType)
        {
            switch (credentialType)
            {
                case CredentialType.Certificate:
                    return new SslBinding();
                case CredentialType.Windows:
                    return new NegotiateBinding();
                default:
                    throw new ArgumentException("credentialType");
            }
        }
    }
}
