using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security;

namespace LazyBones.Scheduling.Calendar
{
    [Serializable]
    class HolidayCalendar : BasicCalendar
    {
        List<DateTimeOffset> holidays = new List<DateTimeOffset>();
        public HolidayCalendar()
        {
        }
        protected HolidayCalendar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            //info.AddValue("startTime", startTime);
            //info.AddValue("endTime", endTime);
            //info.AddValue("invert", Invert);
        }
        public override bool Included(DateTimeOffset time)
        {
            return base.Included(time);
        }
        public override DateTimeOffset GetNextTimeAfter(DateTimeOffset time)
        {
            throw new NotImplementedException();
        }
    }
}
