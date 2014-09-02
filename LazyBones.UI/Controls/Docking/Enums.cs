using System;
using System.ComponentModel;

namespace LazyBones.UI.Controls.Docking
{
    //定义LazyBones.UI.Controls.Docking中用到的枚举值
    /// <summary>
    /// 停靠的状态
    /// </summary>
    public enum DockState
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 浮动停靠
        /// </summary>
        Float,
        /// <summary>
        /// 上端停靠且自动隐藏
        /// </summary>
        TopAutoHide,
        LeftAutoHide,
        BottomAutoHide,
        RightAutoHide,
        /// <summary>
        /// 文档
        /// </summary>
        Document,
        Top,
        Left,
        Bottom,
        Right,
        /// <summary>
        /// 隐藏
        /// </summary>
        Hidden
    }
    /// <summary>
    /// 表示可停靠的位置
    /// </summary>
    [Flags]
    [Serializable]
    [Editor(typeof(LazyBones.UI.Designers.EnumDropDownEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public enum DockAreas
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Document = 16,
        Float = 32,
        AtEdge = Left | Right | Top | Bottom,
        ExceptDocment = Float | Left | Right | Top | Bottom,
        All = Float | Left | Right | Top | Bottom | Document
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DocumentTabStripLocation
    {
        Top = 0,
        Bottom
    }

    public enum AppearanceStyle
    {
        ToolWindow,
        Document
    }

    public enum HitTestArea
    {
        Caption,
        TabStrip,
        Content,
        None
    }
}
