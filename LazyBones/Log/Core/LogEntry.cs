using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Log.Core
{
    using Targets;
    using Filters;
    public class LogEntry
    {
        public LogEntry(Target target, IEnumerable<Filter> filters)
        {
            Target = target;
            Filters = filters.ToArray();
        }
        public LogEntry NextEntry { get; set; }
        public Target Target { get; private set; }
        public Filter[] Filters { get; private set; }
    }
}
