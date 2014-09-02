using LazyBones.Threading;

namespace LazyBones.Log.Targets.Wrappers
{
    internal class TargetAsyncWrapper : Target
    {
        static LBThreadPool threadPool = new LBThreadPool("TargetAsyncWrapperThreadPool");
        Target target;
        public TargetAsyncWrapper(Target target)
        {
            this.target = target;
        }
        protected override void Write(LogEvent logEvent)
        {
            threadPool.RunAsync(target.WriteLogEvent, logEvent);
        }
    }
}
