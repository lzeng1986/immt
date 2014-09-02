using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.UI.Controls.Gantt
{
    static class Helper
    {
        public static TimeSpan GetTimeSpan(this TimeDisplayScale scale, double period)
        {
            switch (scale)
            {
                case TimeDisplayScale.Hour:
                    return TimeSpan.FromHours(period);
                case TimeDisplayScale.Day:
                    return TimeSpan.FromDays(period);
                default:
                    return TimeSpan.Zero;
            }
        }
        public static double GetTimePeriod(this TimeDisplayScale scale, TimeSpan span)
        {
            switch (scale)
            {
                case TimeDisplayScale.Hour:
                    return span.TotalHours;
                case TimeDisplayScale.Day:
                    return span.TotalDays;
                default:
                    return 0;
            }
        }
    }
}
