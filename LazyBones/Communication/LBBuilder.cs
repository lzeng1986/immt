using LazyBones.Communication.Client;
using LazyBones.Communication.Messages;
using LazyBones.Communication.Server;

namespace LazyBones.Communication
{
    public static class LBBuilder
    {
        public static LBServerBuilder<TMessage> Server<TMessage>(SocketMode mode)
            where TMessage : class,ILBMessage
        {
            return new LBServerBuilder<TMessage>(mode);
        }

        public static LBClientBuilder<TMessage> Client<TMessage>(SocketMode mode)
            where TMessage : class,ILBMessage
        {
            return new LBClientBuilder<TMessage>(mode);
        }
    }
}
