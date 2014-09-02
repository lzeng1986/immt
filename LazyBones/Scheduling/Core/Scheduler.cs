using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Scheduling.Core
{
    class Scheduler
    {
        public void Schedule(IJob job,ITrigger trigger)
        {
            var type = job.GetType();

        }
    }
}
