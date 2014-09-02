using System.IO;
using System.Text;
using LazyBones.Log.Config;
using LazyBones.Log.Core;
using LazyBones.Log.Renderers;

namespace LazyBones.Log.Layouts
{
    /// <summary>
    /// 系统提供的默认布局器
    /// </summary>
    [Layout("default")]
    public class DefaultLayout : Layout
    {
        string layoutText;
        string fixedText = null;
        LogConfigItemFactory configItemFactory;
        public DefaultLayout()
            : this(string.Empty)
        {
        }
        /// <summary>
        /// 根据布局文本创建一个新的<see cref="DefaultLayout"/>实例，此方法创建的布局器使用默认实例工厂
        /// </summary>
        /// <param name="layoutText">布局文本</param>
        public DefaultLayout(string layoutText)
            : this(layoutText, LogConfigItemFactory.DefaultFactory)
        {
        }
        /// <summary>
        /// 根据布局文本和实例工厂创建一个新的<see cref="DefaultLayout"/>实例
        /// </summary>
        /// <param name="layoutText">布局文本</param>
        /// <param name="configItemFactory">实例工厂</param>
        internal DefaultLayout(string layoutText, LogConfigItemFactory configItemFactory)
        {
            this.configItemFactory = configItemFactory;
            LayoutText = layoutText;            
        }
        /// <summary>
        /// 此布局器包含的渲染器
        /// </summary>
        public Renderer[] Reanderers { get; private set; }
        protected override void InitializeLayout()
        {            
        }

        protected override void FormatLogEvent(StringBuilder sb, LogEvent logEvent)
        {
            if (fixedText != null)
            {
                sb.Append(fixedText);
                return;
            }
            foreach (var renderer in Reanderers)
            {
                try
                {
                    renderer.Format(sb, logEvent);
                }
                catch (System.Exception ex)
                {
                    ex.Check();
                    TinyLog.Warn("格式化{0}时出错：{1}", renderer,ex);
                }
            }
        }
        /// <summary>
        /// 获取或设置布局文本
        /// </summary>
        public override string LayoutText
        {
            get
            {
                return layoutText;
            }
            set
            {
                layoutText = value;
                Reanderers = LayoutParser.Parse(new StringReader(layoutText), configItemFactory);
                //如果只存在一个渲染器，且为LabelRenderer，则此布局为一个固定的字符串
                if (Reanderers.Length == 1 && Reanderers[0] is LabelRenderer)
                {
                    fixedText = (Reanderers[0] as LabelRenderer).Text;
                }
            }
        }
    }
}
