
namespace LazyBones.Communication.Messages
{
    public class LBTextMessage : LBMessage
    {
        public virtual string Text { get; set; }
        public LBTextMessage()
        {
        }
        public LBTextMessage(string text)
        {
            Text = text;
        }
        public LBTextMessage(string text, string replyId)
            : base(replyId)
        {
            Text = text;
        }
    }
}
