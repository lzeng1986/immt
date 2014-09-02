
namespace LazyBones.Log.Targets
{
    /// <summary>
    /// 表示文件存档方式
    /// </summary>
    public enum FileArchiveMode
    {
        /// <summary>
        /// 不存档
        /// </summary>
        None,

        /// <summary>
        /// 每年存档
        /// </summary>
        Year,

        /// <summary>
        /// 每月存档
        /// </summary>
        Month,

        /// <summary>
        /// 每周存档
        /// </summary>
        Week,

        /// <summary>
        /// 每天存档
        /// </summary>
        Day,

        /// <summary>
        /// 每小时存档
        /// </summary>
        Hour,

        /// <summary>
        /// 每分钟存档
        /// </summary>
        Minute
    }
}
