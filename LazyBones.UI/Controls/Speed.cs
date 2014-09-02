
namespace LazyBones.UI.Controls
{
    /// <summary>
    /// 定义动画显示的速度
    /// </summary>
    public enum Speed : int
    {
        Lowest = 5000,
        Low = 2000,
        Normal = 1000,
        Fast = 500,
        Fastest = 200
    }

    public enum Acions
    {
        FeedIn = 0,
        FeedOut,
        SlideUp,
        SlideDown,
        SlideLeft,
        SlideRight
    }
}
