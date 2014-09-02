using System.Drawing;

namespace LazyBones.UI.Controls.Tree
{
    public class DrawContext
    {
        public Graphics Graphics { get; set; }
        public Rectangle Bounds { get; set; }
        public Font Font { get; set; }
        public DrawSelectionMode DrawSelection { get; set; }
        public bool DrawFocus { get; set; }
        public NodeCell CurrentEditorOwner { get; set; }
        public bool Enabled { get; set; }
    }
}
