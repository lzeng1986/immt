
namespace LazyBones.Log.Config
{
    /// <summary>
    /// 表示可以从配置初始化的对象接口
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// 从<see cref="LogConfig"/>初始化对象
        /// </summary>
        /// <param name="logConfig">配置文件</param>
        void Initialize(LogConfig logConfig);
    }
}
