using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    internal class DummyControl : Control
    {
        public DummyControl()
        {
            SetStyle(ControlStyles.Selectable, false);
            UpdateStyles();
        }
    }
}
