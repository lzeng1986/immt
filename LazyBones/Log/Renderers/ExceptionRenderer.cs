using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LazyBones.Log.Config;

namespace LazyBones.Log.Renderers
{
    [Renderer("exception")]
    class ExceptionRenderer : Renderer
    {
        [DefaultValue(0)]
        public int MaxLevel { get; set; }

        List<Action<StringBuilder, Exception>> appenders = new List<Action<StringBuilder, Exception>>();
        static void GetFormat(string format, List<Action<StringBuilder, Exception>> appender)
        {
            if (string.IsNullOrEmpty(format))
                return;
            foreach (var f in format.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (f.ToLowerInvariant())
                {
                    case "message":
                        appender.Add(Message);
                        break;
                    case "targetsite":
                        appender.Add(Method);
                        break;
                    case "stacktrace":
                        appender.Add(StackTrace);
                        break;
                    case "type":
                        appender.Add(Type);
                        break;
                    case "tostring":
                        appender.Add(ToString);
                        break;
                }
            }
        }
        static void Message(StringBuilder sb, Exception e)
        {
            sb.Append(e.Message);
        }
        static void Method(StringBuilder sb, Exception e)
        {
            if (e.TargetSite != null)
                sb.Append(e.TargetSite.ToString());
        }
        static void StackTrace(StringBuilder sb, Exception e)
        {
            sb.Append(e.StackTrace);
        }
        static void Type(StringBuilder sb, Exception e)
        {
            sb.Append(e.GetType());
        }
        static void ToString(StringBuilder sb, Exception e)
        {
            sb.Append(e.ToString());
        }
        void Append(StringBuilder sb, Exception e)
        {
            appenders[0](sb, e);
            foreach (var a in appenders.Skip(1))
            {
                sb.AppendLine();
                a(sb, e);
            }
        }
        protected override void FormatString(StringBuilder sb, LogEvent logEvent)
        {
            if (logEvent.Exception == null)
                return;

            if (appenders.Count <= 0)
                appenders.Add(ToString);

            var e = logEvent.Exception;
            Append(sb, e);

            e = e.InnerException;
            for (var level = 0; level < MaxLevel && e != null; level++)
            {
                sb.AppendLine();
                Append(sb, e);
                e = e.InnerException;
            }
        }
    }
}
