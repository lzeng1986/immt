using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LazyBones.Linq;
using System.Security;

namespace LazyBones.Scheduling.Calendar
{
    /// <summary>
    /// 实现一个年日历，记录月、日，精确到天
    /// </summary>
    [Serializable]
    public class AnnualCalendar : BasicCalendar
    {
        List<DateTimeOffset> excludeDays = new List<DateTimeOffset>();
        public AnnualCalendar()
        {
            comparer = CmpFactory.Create((Func<DateTimeOffset, DateTimeOffset, int>)ComparerFun);
        }
        protected AnnualCalendar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            excludeDays = (List<DateTimeOffset>)info.GetValue("excludeDays", typeof(List<DateTimeOffset>));
        }
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("excludeDays", excludeDays);
        }
        public DateTimeOffset[] ExcludeDays
        {
            get { return excludeDays.ToArray(); }
        }

        const int ConstYear = 1986; //我出生的那一年，而且是闰年
        IComparer<DateTimeOffset> comparer;
        static int ComparerFun(DateTimeOffset a, DateTimeOffset b)
        {
            if (a.Month == b.Month)
                return a.Day.CompareTo(b.Day);
            return a.Month.CompareTo(b.Month);
        }
        public void AddExcludeDay(DateTimeOffset day)
        {
            day = new DateTimeOffset(ConstYear, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);
            if (excludeDays.BinarySearch(day, comparer) < 0)
            {
                excludeDays.Add(day);
                excludeDays.Sort(comparer);
            }
        }
        public void RemoveExcludeDay(DateTimeOffset day)
        {
            day = new DateTimeOffset(ConstYear, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);
            var ind = excludeDays.BinarySearch(day, comparer);
            if (ind >= 0)
            {
                excludeDays.RemoveAt(ind);
            }
        }
        bool IsDayExcluded(DateTimeOffset day)
        {
            return excludeDays.BinarySearch(day, comparer) >= 0;
        }
        public override bool Included(DateTimeOffset time)
        {
            if (!base.Included(time))
            {
                return true;
            }
            time = TimeZoneInfo.ConvertTime(time, TimeZone);
            return !IsDayExcluded(time);
        }
        public override DateTimeOffset GetNextTimeAfter(DateTimeOffset time)
        {
            var baseTime = baseCalendar == null ? time : baseCalendar.GetNextTimeAfter(time);
            if (baseTime > time)
                time = baseTime;
            time = TimeZoneInfo.ConvertTime(time, TimeZone);
            time = time.Date;
            while (IsDayExcluded(time))
                time.AddDays(1);
            return time;
        }
    }
}
