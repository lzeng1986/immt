
namespace LazyBones.Communication.Messages
{
    public class LBRawDataMessage : LBMessage
    {
        public virtual byte[] RawData { get; set; }

        public LBRawDataMessage()
        {
        }
        public LBRawDataMessage(byte[] rawData)
        {
            RawData = rawData;
        }
        public LBRawDataMessage(byte[] rawData, string replyId)
            : base(replyId)
        {
            RawData = rawData;
        }
    }
}
