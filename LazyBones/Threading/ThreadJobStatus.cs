
namespace LazyBones.Threading
{
    /// <summary>
    /// 表示Job的状态 [10/9/2013 zliang]
    /// </summary>
    public enum ThreadJobStatus
    {
        /// <summary>
        /// <see cref="ThreadJob"/>对象已创建，但没有进入线程池队列，处于等待状态
        /// </summary>
        Created = 0,
        /// <summary>
        /// <see cref="ThreadJob"/>对象已进入线程池队列，等待执行
        /// </summary>
        InQueue = 1,
        /// <summary>
        /// <see cref="ThreadJob"/>对象正在执行
        /// </summary>
        Executing = 2,
        /// <summary>
        /// <see cref="ThreadJob"/>对象正常结束
        /// </summary>
        Finished = 3,
        /// <summary>
        /// <see cref="ThreadJob"/>对象结束，但有错误
        /// </summary>
        Error = 4,
        /// <summary>
        /// <see cref="ThreadJob"/>对象被取消
        /// </summary>
        Canceled = 5
    }
    /// <summary>
    /// 表示处理<see cref="ThreadJob"/>对象完成事件的方法
    /// </summary>
    /// <param name="job">触发该事件的<see cref="ThreadJob"/>对象</param>
    public delegate void FinishedHandler(ThreadJob job);
    /// <summary>
    /// 表示处理<see cref="ThreadJob"/>对象被取消事件的方法
    /// </summary>
    /// <param name="job">触发该事件的<see cref="ThreadJob"/>对象</param>
    public delegate void CanceledHandler(ThreadJob job);
    /// <summary>
    /// 表示处理<see cref="ThreadJob"/>对象开始执行事件的方法
    /// </summary>
    /// <param name="job">触发该事件的<see cref="ThreadJob"/>对象</param>
    public delegate void ExecutedHandler(ThreadJob job);
    /// <summary>
    /// 表示处理<see cref="ThreadJob"/>对象发生错误事件的方法
    /// </summary>
    /// <param name="job">触发该事件的<see cref="ThreadJob"/>对象</param>
    public delegate void ErrorOccuredHandler(ThreadJob job);
}
