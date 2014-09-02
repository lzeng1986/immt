using System;

namespace LazyBones.Scheduling
{
    static class SystemTime
    {
        public static DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }
        public static DateTimeOffset Now
        {
            get { return DateTimeOffset.Now; }
        }
    }
}
