using System;

namespace LazyBones.Scheduling
{
    /// <summary>
    /// 日历的接口
    /// </summary>
    public interface ICalendar
    {
        /// <summary>
        /// 获取或设置日历的名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 获取或设置日历的描述
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// 获取或设置基础日历
        /// </summary>
        ICalendar BaseCalendar { set; get; }
        /// <summary>
        /// 获取或设置日历所属的时区
        /// </summary>
        TimeZoneInfo TimeZone { set; get; }
        /// <summary>
        /// 确定日历是否包含一个指定的时间点
        /// </summary>
        /// <param name="time">指定的时间点</param>
        /// <returns>是否包含</returns>
        bool Included(DateTimeOffset time);
        /// <summary>
        /// 获取日历中包含的指定时间点之后的下一个时间点，如果不存在，则返回<see cref="DateTimeOffset.MinValue"/>
        /// </summary>
        /// <param name="time">指定时间点</param>
        /// <returns></returns>
        DateTimeOffset GetNextTimeAfter(DateTimeOffset time);
        /// <summary>
        /// 获取日历中包含的当前时间之后的下一个时间点，如果不存在，则返回<see cref="DateTimeOffset.MinValue"/>
        /// </summary>
        DateTimeOffset GetNextTime();
    }
}
