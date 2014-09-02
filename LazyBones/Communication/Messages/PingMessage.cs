
namespace LazyBones.Communication.Messages
{
    public class PingMessage : LBMessage
    {
        public PingMessage()
        {
        }
        public PingMessage(string replyId)
            : base(replyId)
        {
        }
    }
}
