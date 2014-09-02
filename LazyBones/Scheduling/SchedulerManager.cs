using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Scheduling.Core;

namespace LazyBones.Scheduling
{
    public static class SchedulerManager
    {
        static Scheduler scheduler = new Scheduler();
        public static void Schedule(IJob job, ITrigger trigger)
        {
            scheduler.Schedule(job, trigger);
        }
    }
}
