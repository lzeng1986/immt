
namespace LazyBones.Threading
{
    /// <summary>
    /// 表示线程池的信息
    /// </summary>
    public class ThreadPoolInfo
    {
        /// <summary>
        /// 获取线程池当前处于工作状态的线程数
        /// </summary>
        public int WorkingThreadsCount { get; internal set; }
        /// <summary>
        /// 获取线程池内线程总数量
        /// </summary>
        public int ThreadsInPoolCount { get; internal set; }
        /// <summary>
        /// 获取线程池内作业总数量，包括正在执行和等待执行
        /// </summary>
        public int JobInPoolCount { get; internal set; }
        /// <summary>
        /// 获取线程池内等待执行的作业数量
        /// </summary>
        public int JobInQueueCount { get; internal set; }
    }
}
