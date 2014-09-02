
namespace LazyBones.Scheduling
{
    /// <summary>
    /// 定义执行实例应实现的接口
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="context">当前实例执行的上下文</param>
        void Execute(JobExecutionContext context);
    }
}
