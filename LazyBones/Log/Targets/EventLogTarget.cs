using System.ComponentModel;
using System.Diagnostics;
using LazyBones.Config;
using LazyBones.Log.Config;

namespace LazyBones.Log.Targets
{
    [Target("eventLog")]
    class EventLogTarget : TargetWithLayout
    {
        EventLog eventLog;
        public EventLogTarget()
        {
        }
        public string Source { get; set; }
        [DefaultValue(".")]
        public string MachineName { get; set; }
        [DefaultValue("Application")]
        public string LogName { get; set; }
        protected override void InitializeTarget()
        {
            Source = AppDomainWrapper.FriendlyName;
            if (EventLog.SourceExists(Source, MachineName))
            {
                string currentLogName = EventLog.LogNameFromSourceName(this.Source, this.MachineName);
                if (currentLogName != this.LogName)
                {
                    // re-create the association between Log and Source
                    EventLog.DeleteEventSource(this.Source, this.MachineName);

                    var escd = new EventSourceCreationData(this.Source, this.LogName)
                    {
                        MachineName = this.MachineName
                    };

                    EventLog.CreateEventSource(escd);
                }
            }
            else
            {
                var data = new EventSourceCreationData(Source, LogName)
                {
                    MachineName = this.MachineName
                };
                EventLog.CreateEventSource(data);
            }
            eventLog = new EventLog(Source, MachineName, LogName);
        }
        protected override void Write(LogEvent logEvent)
        {
            eventLog.WriteEntry(Body.GetFormatMessage(logEvent));
        }
        protected override void CloseTarget()
        {
            eventLog.Dispose();
        }
    }
}
