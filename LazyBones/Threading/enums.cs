
namespace LazyBones.Threading
{
    /// <summary>
    /// 表示<see cref="Task"/>的状态 [10/9/2013 zliang]
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// <see cref="Task"/>对象已进入线程池队列，等待执行
        /// </summary>
        Waiting,
        /// <summary>
        /// <see cref="Task"/>对象正在执行
        /// </summary>
        Working,
        /// <summary>
        /// <see cref="Task"/>对象正常结束
        /// </summary>
        Completed,
        /// <summary>
        /// <see cref="Task"/>对象被取消
        /// </summary>
        Canceled
    }

    /// <summary>
    /// 表示<see cref="Task"/>的在<see cref="ITaskExecutor"/>内执行的优先级
    /// </summary>
    public enum TaskPriority
    {     
        Lowest = 0,
        Lower = 1,
        Normal = 2,
        Higher = 3,
        Highest = 4,
    }
    public enum CompleteReason
    {
        Normal,
        Canceled,
        Error
    }
}
