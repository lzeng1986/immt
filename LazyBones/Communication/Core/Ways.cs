
namespace LazyBones.Communication.Core
{
    /// <summary>
    /// 表示通讯方式
    /// </summary>
    public enum Ways
    {
        /// <summary>
        /// 可发送
        /// </summary>
        Send = 1,
        /// <summary>
        /// 可接收
        /// </summary>
        Receive = 2,
        /// <summary>
        /// 可双向通讯，等于 Send | Receive
        /// </summary>
        Duplex = Send | Receive
    }
}
