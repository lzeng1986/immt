using System;
using System.Linq;
using System.Runtime.Serialization;

namespace LazyBones.Scheduling.Calendar
{
    /// <summary>
    /// 表示一个月日历，用于描述一个月内天的集合，该日历没有考虑月份的总天数
    /// </summary>
    public class MonthlyCalendar : BasicCalendar
    {
        const int MaxDaysInMonth = 31;
        bool[] excludeDays = new bool[MaxDaysInMonth];
        bool excludeAll = false;
        public MonthlyCalendar()
        {
        }
        protected MonthlyCalendar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            excludeAll = info.GetBoolean("excludeAll");
            excludeDays = (bool[])info.GetValue("excluedDays", typeof(bool[]));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("excludeAll", excludeAll);
            info.AddValue("excluedDays", excludeDays);
        }
        public void SetDayInclude(int day)
        {
            if (day < 1 || MaxDaysInMonth < day)
                throw new ArgumentException("天数必须在1和31之间", "day");
            excludeDays[day - 1] = false;
            excludeAll = false;
        }
        public void SetDayExclude(int day)
        {
            if (day < 1 || MaxDaysInMonth < day)
                throw new ArgumentException("天数必须在1和31之间", "day");
            excludeDays[day - 1] = true;
            excludeAll = excludeDays.All(d=>d);
        }
        public override bool Included(DateTimeOffset time)
        {
            if (excludeAll)
                return false;

            if (!base.Included(time))
                return false;

            time = TimeZoneInfo.ConvertTime(time, TimeZone);
            return !excludeDays[time.Day - 1];
        }
        public override DateTimeOffset GetNextTimeAfter(DateTimeOffset time)
        {
            if (excludeAll)
                return DateTimeOffset.MinValue;

            var baseTime = baseCalendar == null ? time : baseCalendar.GetNextTimeAfter(time);
            if (baseTime > time)
                time = baseTime;

            time = TimeZoneInfo.ConvertTime(time, TimeZone);
            time = time.Date;
            while (excludeDays[time.Day - 1])
                time.AddDays(1);
            return time;
        }
    }
}
