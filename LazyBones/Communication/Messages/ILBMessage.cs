
namespace LazyBones.Communication.Messages
{
    public interface ILBMessage
    {
        string Id { get; set; }
        string ReplyId { get; set; }
    }
}
