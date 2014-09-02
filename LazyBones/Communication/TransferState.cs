
namespace LazyBones.Communication
{
    /// <summary>
    /// 表示传输状态
    /// </summary>
    public enum TransferState
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle,
        /// <summary>
        /// 正在发送
        /// </summary>
        Sending,
        /// <summary>
        /// 正在接收
        /// </summary>
        Receiving
    }
}
