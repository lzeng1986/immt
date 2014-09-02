
namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 定义一些常量
    /// </summary>
    public static class Const
    {
        public const int SplitterSize = 4;
        public const int MinGridSize = 24;
        /// <summary>
        /// 自动隐藏状态集合
        /// </summary>
        public static readonly DockState[] AutoHideDockStates = 
        { DockState.LeftAutoHide, DockState.RightAutoHide, DockState.TopAutoHide, DockState.BottomAutoHide };
    }
}
