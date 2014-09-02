using System;
using System.Runtime.Serialization;
using System.Security;

namespace LazyBones.Scheduling.Calendar
{
    /// <summary>
    /// 实现一个日历的抽象基类
    /// </summary>
    [Serializable]
    public abstract class BasicCalendar : ICalendar, ISerializable
    {
        protected BasicCalendar()
        {
        }
        protected BasicCalendar(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("name");
            Description = info.GetString("description");
            baseCalendar = (ICalendar)info.GetValue("baseCalendar", typeof(ICalendar));
            timeZone = (TimeZoneInfo)info.GetValue("timeZone", typeof(TimeZoneInfo));
        }
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", Name);
            info.AddValue("description", Description);
            info.AddValue("baseCalendar", baseCalendar);
            info.AddValue("timeZone", timeZone);
        }
        /// <summary>
        /// 获取或设置日历的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 获取或设置日历的描述
        /// </summary>
        public string Description { get; set; }

        protected ICalendar baseCalendar = null;
        /// <summary>
        /// 获取或设置基础日历
        /// </summary>
        public ICalendar BaseCalendar
        {
            get { return baseCalendar; }
            set { baseCalendar = value; }
        }
        TimeZoneInfo timeZone = TimeZoneInfo.Local;
        /// <summary>
        /// 获取或设置日历所属的时区
        /// </summary>
        public TimeZoneInfo TimeZone
        {
            get { return timeZone; }
            set { timeZone = value; }
        }
        /// <summary>
        /// 确定日历是否包含一个指定的时间点
        /// </summary>
        /// <param name="time">指定的时间点</param>
        /// <returns>是否包含</returns>
        public virtual bool Included(DateTimeOffset time)
        {
            if (time == DateTimeOffset.MinValue)
                throw new ArgumentException("time没有设置值","time");

            return baseCalendar == null ? true : baseCalendar.Included(time);
        }
        /// <summary>
        /// 获取日历中包含的指定时间点之后的下一个时间点
        /// </summary>
        /// <param name="time">指定时间点</param>
        /// <returns></returns>
        public abstract DateTimeOffset GetNextTimeAfter(DateTimeOffset time);
        /// <summary>
        /// 获取日历中包含的当前时间之后的下一个时间点
        /// </summary>
        public virtual DateTimeOffset GetNextTime()
        {
            return GetNextTimeAfter(SystemTime.Now);
        }
    }
}
