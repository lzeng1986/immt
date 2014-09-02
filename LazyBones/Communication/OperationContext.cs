using System;
using LazyBones.Communication.Server;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication
{
    //表示操作的上下文
    public class OperationContext : IDisposable
    {
        [ThreadStatic]
        static OperationContext current;

        public static OperationContext Current
        {
            get { return current; }
        }

        ServerClientSession contextSession;
        internal OperationContext(ServerClientSession contextSession)
        {
            this.contextSession = contextSession;
        }

        public ILBSessionContext ContextSession
        {
            get { return contextSession; }
        }

        public ICommunicator Channel
        {
            get { return contextSession.CommunicationChannel; }
        }

        public TAppServer GetAppServerInstance<TAppServer>() where TAppServer : IAppServer
        {
            return contextSession.GetAppServerInstance<TAppServer>();
        }

        public void SendMessage(ILBMessage message)
        {
            contextSession.SendMessage(message);
        }

        internal static IDisposable SwichContext(ServerClientSession session)
        {
            return new ContextHolder(new OperationContext(session));
        }
        public void Dispose()
        {
            current = null;
        }
        class ContextHolder : IDisposable
        {
            OperationContext old;
            public ContextHolder(OperationContext @new)
            {
                old = current;
                current = @new;
            }
            public void Dispose()
            {
                current = old;
            }
        }
    }
}
