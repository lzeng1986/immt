
namespace LazyBones.Communication.Apps.NfsV2
{
    class NfsV2Binding : LBBinding
    {
        public override Protocols.ILBProtocol CreateProtocol()
        {
            return new RpcV2Protocol();
        }

        public override IAppSession CreateAppSession(Server.ILBSessionContext contextSession)
        {
            return new NfsV2Session();
        }
    }
}
