using LazyBones.Communication.Security;
using LazyBones.Communication.Protocols;

namespace LazyBones.Communication
{
    /// <summary>
    /// 定义系统默认值
    /// </summary>
    public static class Defaults
    {
        public const int SendTimeOut = 0;
        public const int ReceiveTimeOut = 0;
        public const int SendBufferSize = 2048;
        public const int ReceiveBufferSize = char.MaxValue;
        public const int MaxConnection = 100;
        public const int IdleSessionTimeOut = 300;
        public const int KeepAliveTimeOut = 1000;
        public const int ConnectTimeOut = 1000;
        public const SocketMode Mode = SocketMode.Tcp;
        public const int WaitResponseTimeOut = 60 * 1000;
        public const int SerializeLength = 65536;
        public const int GraphObjectSize = 65536;
        public const CredentialType DefaultCredentialType = CredentialType.None;

        static ILBProtocol defaultProtocol;
        public static ILBProtocol Default
        {
            get { return defaultProtocol ?? (defaultProtocol = new BinarySerializationProtocol()); }
        }
    }
}
