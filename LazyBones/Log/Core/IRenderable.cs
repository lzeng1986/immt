
namespace LazyBones.Log.Core
{
    /// <summary>
    /// 定义可渲染对象接口
    /// </summary>
    internal interface IRenderable
    {
        /// <summary>
        /// 将<see cref="LogEvent"/>对象格式化成日志记录消息
        /// </summary>
        /// <param name="logEvent">格式化对象</param>
        /// <returns>日志消息</returns>
        string GetFormatMessage(LogEvent logEvent);
    }
}
