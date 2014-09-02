using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace LazyBones.Scheduling.Calendar
{
    /// <summary>
    /// 实现一个周日历，表示一周七天
    /// </summary>
    [Serializable]
    public class WeeklyCalendar : BasicCalendar
    {
        bool[] excludeDays = new bool[7];
        bool excludeAll = false;
        public WeeklyCalendar()
        {
            excludeDays[(int)DayOfWeek.Saturday] = true;
            excludeDays[(int)DayOfWeek.Sunday] = true;
        }
        protected WeeklyCalendar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            excludeAll = info.GetBoolean("excludeAll");
            excludeDays = (bool[])info.GetValue("excludeDays", typeof(bool[]));
        }
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("excludeAll", excludeAll);
            info.AddValue("excludeDays", excludeDays);
        }
        public void IncludeDay(DayOfWeek day)
        {
            excludeDays[(int)day] = false;
            excludeAll = false;
        }
        public void ExcludeDay(DayOfWeek day)
        {
            excludeDays[(int)day] = true;
            excludeAll = excludeDays.All(d => d);
        }
        public bool[] DaysExcluded
        {
            get { return excludeDays; }
        }
        public override bool Included(DateTimeOffset time)
        {
            if (excludeAll)
                return false;

            if (!base.Included(time))
                return false;
            
            time = TimeZoneInfo.ConvertTime(time, TimeZone);
            return !excludeDays[(int)time.DayOfWeek];
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
            while (excludeDays[(int)time.DayOfWeek])
                time.AddDays(1);
            return time;
        }
    }
}
