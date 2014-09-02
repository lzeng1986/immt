using System;
using System.Runtime.Serialization;
using System.Security;

namespace LazyBones.Scheduling.Calendar
{
    /// <summary>
    /// 实现一个天日历，表示一天中的一个时间段，精确到毫秒
    /// </summary>
    [Serializable]
    public class DailyCalendar : BasicCalendar
    {
        TimeSpan startTime, endTime;
        public DailyCalendar(TimeSpan rangeStartTime, TimeSpan rangeEndTime)
        {
            Validate(rangeStartTime, "rangeStartTime");
            Validate(rangeEndTime, "rangeEndTime");
            if (rangeStartTime >= rangeEndTime)
                throw new ArgumentException("起始时刻应早于结束时刻");
            startTime = rangeStartTime;
            endTime = rangeEndTime;
        }
        protected DailyCalendar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            startTime = (TimeSpan)info.GetValue("startTime", typeof(TimeSpan));
            endTime = (TimeSpan)info.GetValue("endTime", typeof(TimeSpan));
            Invert = info.GetBoolean("invert");
        }
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("startTime", startTime);
            info.AddValue("endTime", endTime);
            info.AddValue("invert", Invert);
        }
        void Validate(TimeSpan timeSpan, string paramName)
        {
            if (timeSpan <= EndTimeOfDay)
                return;
            throw new ArgumentException("时间长度不能超过一天", paramName);
        }
        /// <summary>
        /// 获取或设置是否反转时间区间
        /// </summary>
        public bool Invert { get; set; }
        static readonly TimeSpan EndTimeOfDay = new TimeSpan(0, 23, 59, 59, 999);   //一天的时间长度，精确到毫秒
        public override bool Included(DateTimeOffset time)
        {
            if (!base.Included(time))
                return false;
            var timeOfDay = TimeZoneInfo.ConvertTime(time, TimeZone).TimeOfDay;
            if (Invert)
            {
                return (TimeSpan.Zero <= timeOfDay && timeOfDay < startTime) || (endTime < timeOfDay && timeOfDay <= EndTimeOfDay);
            }
            else
            {
                return startTime <= timeOfDay && timeOfDay <= endTime;
            }
        }
        public override DateTimeOffset GetNextTimeAfter(DateTimeOffset time)
        {
            var nextTime = time.AddMilliseconds(1);
            while (!Included(nextTime))
            {
                var timeOfDay = nextTime.TimeOfDay;
                var date = nextTime.Date;
                if (Invert)
                {
                    if(startTime <= timeOfDay && timeOfDay <= endTime)
                        nextTime = date + endTime;
                    else if (baseCalendar != null)
                        nextTime = baseCalendar.GetNextTimeAfter(time);
                    else
                        nextTime = nextTime.AddMilliseconds(1);
                }
                else
                {
                    if (timeOfDay < startTime)
                        nextTime = date + startTime;
                    else if (endTime < timeOfDay)
                        nextTime = date.AddDays(1) + startTime;
                    else if (baseCalendar != null)
                        nextTime = baseCalendar.GetNextTimeAfter(time);
                    else
                        nextTime = nextTime.AddMilliseconds(1);
                }
            }
            return nextTime;
        }
        public override string ToString()
        {
            if (Invert)
                return string.Format("[{0}~{1},{2}~{3}]", TimeSpan.Zero, startTime, endTime, EndTimeOfDay);
            else
                return string.Format("[{0}~{1}]", startTime, endTime);
        }
    }
}
