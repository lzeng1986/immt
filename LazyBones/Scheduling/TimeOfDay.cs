using System;
using System.Collections.Generic;

namespace LazyBones.Scheduling
{
    /// <summary>
    /// 表示一天中的时间
    /// </summary>
    [Serializable]
    public class TimeOfDay : IComparable<TimeOfDay>, IEquatable<TimeOfDay>
    {
        int hour;
        int minute;
        int second;
        int millisecond;

        public TimeOfDay(int hour, int minute, int second)
            :this(hour,minute,second,0)
        { 
        }

        public TimeOfDay(int hour, int minute, int second, int millisecond)
        {
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.millisecond = millisecond;
        }

        void Validate()
        {
            if (hour < 0 || 23 < hour)
            {
                throw new ArgumentException("小时必须在0和23之内");
            }

            if (minute < 0 || 59 < minute)
            {
                throw new ArgumentException("分钟必须在0和59之内");
            }

            if (second < 0 || 59 < second)
            {
                throw new ArgumentException("秒必须在0和59之内");
            }
            if (millisecond < 0 || 999 < millisecond)
            {
                throw new ArgumentException("毫秒必须在0和999之内");
            }
        }

        public int Hour
        {
            get { return hour; }
        }
        public int Minute
        {
            get { return minute; }
        }
        public int Second
        {
            get { return second; }
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as TimeOfDay);
        }
        public override int GetHashCode()
        {
            return (hour + 1) ^ (minute + 1) ^ (second + 1);
        }
        public override string ToString()
        {
            return "[" + hour + ":" + minute + ":" + second + "]";
        }
        public bool Equals(TimeOfDay other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return (other.hour == hour && other.minute == minute && other.second == second);
        }
        public int CompareTo(TimeOfDay other)
        {
            if (hour == other.hour)
            {
                if (minute == other.minute)
                    return second.CompareTo(other.second);
                return minute.CompareTo(other.minute);
            }
            return hour.CompareTo(other.hour);
        }
        public static implicit operator TimeOfDay(DateTimeOffset datetime)
        {
            return new TimeOfDay(datetime.Hour, datetime.Minute, datetime.Second);
        }
        public static TimeOfDay Now
        {
            get { return SystemTime.Now; }
        }
    }
}
