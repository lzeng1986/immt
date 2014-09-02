using System;
using LazyBones.Log.Config;
using LazyBones.Config;

namespace LazyBones.Log.Targets
{
    [Target("console")]
    class ConsoleTarget : TargetWithLayout
    {
        protected override void Write(LogEvent logEvent)
        {
            if (Header != null)
            {
                Console.WriteLine(base.Header.GetFormatMessage(logEvent));
            }
            Console.WriteLine(base.Body.GetFormatMessage(logEvent));
            if (Footer != null)
            {
                Console.WriteLine(base.Footer.GetFormatMessage(logEvent));
            }
        }
    }
}
