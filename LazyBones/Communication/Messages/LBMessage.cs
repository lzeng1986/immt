using System;

namespace LazyBones.Communication.Messages
{
    public class LBMessage : ILBMessage
    {
        public string Id { get; set; }
        public string ReplyId { get; set; }

        public LBMessage()
        {
            Id = Guid.NewGuid().ToString();
        }
        public LBMessage(string replyId)
            :this()
        {
            ReplyId = replyId;
        }
    }
}
