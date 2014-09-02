
namespace LazyBones.Log.Targets
{
    /// <summary>
    /// 表示换行符的格式
    /// </summary>
    public enum NewLineMode
    {
        /// <summary>
        /// 空换行符，等于<see cref="System.String.Empty"/>
        /// </summary>
        None,
        /// <summary>
        /// 默认换行符，等于<see cref="System.Environment.NewLine"/>
        /// </summary>
        Default,
        /// <summary>
        /// CR换行符，等于"\r"
        /// </summary>
        CR,
        /// <summary>
        /// LF换行符，等于"\n"
        /// </summary>
        LF,
        /// <summary>
        /// CRLF换行符，等于"\r\n"
        /// </summary>
        CRLF
    }
}
