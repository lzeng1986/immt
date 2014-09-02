using System;
using System.Threading;
using LazyBones.Scheduling.Core;

namespace LazyBones.Scheduling
{
    public interface ITrigger
    {
        Key TriggerKey { get; }
        /// <summary>
        /// 获取或设置触发器名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 获取或设置触发器的描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 获取或设置触发器的优先级
        /// </summary>
        ThreadPriority Priority { get; set; }
        /// <summary>
        /// 获取或设置触发器的开始时间，如果开始时间小于当前时间，则立即开始
        /// </summary>
        DateTimeOffset StartTime { get; set; }
        /// <summary>
        /// 获取或设置触发器的结束时间，如果为null，则一直运行
        /// </summary>
        DateTimeOffset? EndTime { get; set; }
        /// <summary>
        /// 获取最后触发时间，如果为null则表示没有
        /// </summary>
        DateTimeOffset? FinalFireTime { get; }
        /// <summary>
        /// 获取下一个触发时间，如果为null则表示没有
        /// </summary>
        DateTimeOffset? NextFireTime { get; }
        /// <summary>
        /// 获取前一个触发时间，如果为null则表示没有
        /// </summary>
        DateTimeOffset? PrevFireTimeUtc { get; }
        /// <summary>
        /// 获取在指定时间点时候的触发时间，如果为null则表示没有
        /// </summary>
        /// <param name="time">指定的时间点</param>
        DateTimeOffset? GetFireTimeAfter(DateTimeOffset? time);
        /// <summary>
        /// 获取触发器是否还会触发
        /// </summary>
        bool MayFireAgain { get; }
    }
}
