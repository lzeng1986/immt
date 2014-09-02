using LazyBones.Log.Config;
using LazyBones.Config;

namespace LazyBones.Log.Targets
{
    [Target("coloredConsole")]
    [Static]
    class ColoredConsoleTarget : TargetWithLayout
    {

        protected override void Write(LogEvent logEvent)
        {
            
        }
    }
}
