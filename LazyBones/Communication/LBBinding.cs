using LazyBones.Communication.Protocols;
using LazyBones.Communication.Server;
using LazyBones.Communication.Security;

namespace LazyBones.Communication
{
    public abstract class LBBinding
    {
        protected LBBinding()
        {
            SocketMode = SocketMode.Tcp;
            SyncSend = true;
            Backlog = 100;
            MaxConnection = Defaults.MaxConnection;
            ReceiveBufferSize = Defaults.ReceiveBufferSize;
            ReceiveTimeOut = Defaults.ReceiveTimeOut;
            SendTimeOut = Defaults.SendTimeOut;
            IdleSessionTimeOut = Defaults.IdleSessionTimeOut;
            KeepAliveTimeOut = Defaults.KeepAliveTimeOut;
            MaxGraphObjectSize = Defaults.GraphObjectSize;
            ConnectTimeOut = Defaults.ConnectTimeOut;
        }

        public virtual SocketMode SocketMode { get; protected set; }

        public virtual bool SyncSend { get; protected set; }

        public virtual int Backlog { get; protected set; }

        public virtual int MaxConnection { get; protected set; }

        public virtual int ReceiveBufferSize { get; protected set; }

        public virtual int ReceiveTimeOut { get; protected set; }

        public virtual int SendTimeOut { get; protected set; }

        public virtual int IdleSessionTimeOut { get; protected set; }

        public virtual int ConnectTimeOut { get; protected set; }

        public virtual int KeepAliveTimeOut { get; protected set; }

        public virtual int MaxGraphObjectSize { get; protected set; }//返系列化支持的最大对象大小

        public abstract ILBProtocol CreateProtocol();

        public abstract IAppSession CreateAppSession(ILBSessionContext contextSession);
    }
    public class TcpConfig
    {
        public int Backlog { get; set; }
        public bool UseStreamMode { get; set; }
        public bool EncryptTransfer { get; set; }
        public CredentialType CredentialType { get; set; }
    }
}
