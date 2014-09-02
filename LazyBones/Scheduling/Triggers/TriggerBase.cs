using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LazyBones.Scheduling.Core;

namespace LazyBones.Scheduling.Triggers
{
    public abstract class TriggerBase : ITrigger
    {
        public Key TriggerKey
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public System.Threading.ThreadPriority Priority
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTimeOffset StartTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTimeOffset? EndTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTimeOffset? FinalFireTime
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset? NextFireTime
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset? PrevFireTimeUtc
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset? GetFireTimeAfter(DateTimeOffset? time)
        {
            throw new NotImplementedException();
        }

        public bool MayFireAgain
        {
            get { throw new NotImplementedException(); }
        }
    }
}
