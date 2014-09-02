using System;

namespace LazyBones.UI.Controls.Docking
{
    public class DockContentEventArgs : EventArgs
    {
        public DockContentEventArgs(IDockContent content)
        {
            Content = content;
        }
        public IDockContent Content { get; private set; }
    }
}
