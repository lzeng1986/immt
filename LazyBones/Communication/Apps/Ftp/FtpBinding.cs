
namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpBinding : LBBinding
    {
        public override Protocols.ILBProtocol CreateProtocol()
        {
            return new FtpProtocol();
        }

        public override IAppSession CreateAppSession(Server.ILBSessionContext contextSession)
        {
            return new FtpSession();
        }
    }
}
