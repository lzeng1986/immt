using System.Text;
using LazyBones.Log.Config;
using LazyBones.Log.Core;

namespace LazyBones.Log.Layouts
{
    public abstract class LayoutBase : IRenderable, IInitializable
    {
        bool intialized = false;
        protected LogConfig Config { get; private set; }
        public string GetFormatMessage(LogEvent logEvent)
        {
            if (!intialized)
            {
                InitializeLayout();
            }
            var sb = new StringBuilder();
            FormatLogEvent(sb, logEvent);
            return sb.ToString();
        }
        public void Initialize(LogConfig logConfig)
        {
            if (intialized)
                return;
            Config = logConfig;
            try
            {
                InitializeLayout();
            }
            catch (System.Exception ex)
            {
                TinyLog.Error("初始化布局器<{0}>时出现错误:{1}", this.GetType(), ex);
            }
            intialized = true;
        }
        public virtual string LayoutText { get; set; }
        protected virtual void InitializeLayout()
        {
        }
        protected abstract void FormatLogEvent(StringBuilder sb, LogEvent logEvent);

        public static LayoutBase Create(string text)
        {
            return new DefaultLayout(text);
        }
        internal static LayoutBase Create(string text, LogConfigItemFactory configItemFactory)
        {
            return new DefaultLayout(text, configItemFactory);
        }
        public static implicit operator LayoutBase(string text)
        {
            return Create(text);
        }
    }
}
