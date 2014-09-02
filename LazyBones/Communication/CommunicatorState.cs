
namespace LazyBones.Communication
{
    /// <summary>
    /// 表示通道状态
    /// </summary>
    public enum CommunicatorState
    {
        /// <summary>
        /// 对象以创建
        /// </summary>
        Created,
        /// <summary>
        /// 对象正在打开
        /// </summary>
        Opening,
        /// <summary>
        /// 对象已打开
        /// </summary>
        Opened,
        /// <summary>
        /// 对象正在关闭
        /// </summary>
        Closing,
        /// <summary>
        /// 对象已关闭
        /// </summary>
        Closed,
        /// <summary>
        /// 对象处于错误状态
        /// </summary>
        Error
    }
    
}
