
namespace LazyBones.Win32
{
    /// <summary>
    /// 消息WM_NCHITTEST返回的结果
    /// </summary>
    public enum HitTest
    {
        Error            = -2,
        Transparent      = -1,
        Nowhere          = 0,
        Client           = 1,
        Caption          = 2,
        Sysmenu          = 3,
        Growbox          = 4,
        Size             = Growbox,
        Menu             = 5,
        HScroll          = 6,
        VScroll          = 7,
        MinButton        = 8,
        MaxButton        = 9,
        Left             = 10,
        Right            = 11,
        Top              = 12,
        TopLeft          = 13,
        TopRight         = 14,
        Bottom           = 15,
        BottomLeft       = 16,
        BottomRight      = 17,
        Border           = 18,
        Reduce           = MinButton,
        Zoom             = MaxButton,
        SizeFirst        = Left,
        SizeLast         = BottomRight,
        Object           = 19,
        Close            = 20,
        Help             = 21
    }
}
