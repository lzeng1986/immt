using System;
using System.Drawing;

namespace LazyBones.UI.Controls.Editor
{
    public class ToolTipRequestEventArgs : EventArgs
    {
        public Point MousePosition { get; private set; }

        public TextLocation LogicalPosition { get; private set; }

        public bool InDocument { get; private set; }

        public bool ToolTipShown
        {
            get { return ToolTipText != null; }
        }

        internal string ToolTipText { get; set; }

        public ToolTipRequestEventArgs(Point mousePosition, TextLocation logicalPosition, bool inDocument)
        {
            MousePosition = mousePosition;
            LogicalPosition = logicalPosition;
            InDocument = inDocument;
        }
    }
}
