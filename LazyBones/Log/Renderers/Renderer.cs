using System;
using System.Text;
using LazyBones.Log.Config;
using LazyBones.Log.Core;

namespace LazyBones.Log.Renderers
{
    /// <summary>
    /// 日志记录渲染器抽象基类
    /// </summary>
    public abstract class Renderer : IRenderable, IInitializable, IDisposable
    {
        bool isIntialized = false;
        /// <summary>
        /// 获取从配置文件加载的配置
        /// </summary>
        protected LogConfig LoggingConfig { get; private set; }
        /// <summary>
        /// 将<see cref="LogEvent"/>对象格式化成字符串
        /// </summary>
        /// <param name="logEvent">格式化对象</param>
        /// <returns>格式化的字符串</returns>
        public string GetFormatMessage(LogEvent logEvent)
        {
            var sb = new StringBuilder();
            return Format(sb, logEvent);
        }
        //此函数是为了提高性能设计，在程序集内部使用，避免重复创建StringBuilder对象
        internal string Format(StringBuilder sb, LogEvent logEvent)
        {
            if (!isIntialized)
            {
                InitializeRenderer();
            }
            FormatString(sb, logEvent);
            return sb.ToString();
        }
        /// <summary>
        /// 初始化渲染器
        /// </summary>
        /// <param name="loggingConfig">从配置文件加载的配置</param>
        public void Initialize(LogConfig loggingConfig)
        {
            if (isIntialized)
            {
                return;
            }
            LoggingConfig = loggingConfig;
            InitializeRenderer();
            isIntialized = true;
        }

        public void Dispose()
        {

        }

        protected virtual void InitializeRenderer()
        {
        }
        /// <summary>
        /// 用于将<see cref="LogEvent"/>对象格式化为文本
        /// </summary>
        /// <param name="sb">用于存储格式化文本的缓冲区</param>
        /// <param name="logEvent">需要格式化的对象</param>
        protected abstract void FormatString(StringBuilder sb, LogEvent logEvent);
    }
}
