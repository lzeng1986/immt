using System;
using LazyBones.Scheduling.Core;

namespace LazyBones.Scheduling
{
    /// <summary>
    /// 方法执行的上下文
    /// </summary>
    public class JobExecutionContext
    {
        public ITrigger Trigger { get; private set; }
        public ICalendar Calendar { get; private set; }
        public int FireCount { get; private set; }
        public LazyBones.Data.DataMap DataMap { get; private set; }
        public IJob JobInstance { get; private set; }
        public DateTimeOffset? ActualFireTime { get; private set; }
        public DateTimeOffset? ScheduledFireTime { get; private set; }
        public DateTimeOffset? PrevFireTime { get; private set; }
        public DateTimeOffset? NextFireTime { get; private set; }
        public object Result { get; set; }
        public TimeSpan JobExecutionTime { get; private set; }
    }
}
