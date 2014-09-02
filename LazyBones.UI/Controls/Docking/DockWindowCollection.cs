using System;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    internal class DockWindowCollection
    {
        internal DockWindow[] dockWindows;
        DockPanel dockPanel;
        internal DockWindowCollection(DockPanel dockPanel)
        {
            dockWindows = new[]
            { 
                new DockWindow(dockPanel, DockStyle.Fill),
                new DockWindow(dockPanel, DockStyle.Left),
                new DockWindow(dockPanel, DockStyle.Right),
                new DockWindow(dockPanel, DockStyle.Top),
                new DockWindow(dockPanel, DockStyle.Bottom)
            };
            this.dockPanel = dockPanel;
        }
        public DockWindow this[DockStyle dockStyle]
        {
            get
            {
                switch (dockStyle)
                {
                    case DockStyle.Fill:
                        return dockWindows[0];
                    case DockStyle.Left:
                        return dockWindows[1];
                    case DockStyle.Right:
                        return dockWindows[2];
                    case DockStyle.Top:
                        return dockWindows[3];
                    case DockStyle.Bottom:
                        return dockWindows[4];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int GetDockWindowSize(DockStyle dockStyle)
        {
            if (dockStyle == DockStyle.Left || dockStyle == DockStyle.Right)
            {
                var width = dockPanel.DockBounds.Width;
                var dockLeftSize = this[DockStyle.Left].AutoHidePortion >= 1
                    ? (int)this[DockStyle.Left].AutoHidePortion
                    : (int)(width * this[DockStyle.Left].AutoHidePortion);
                var dockRightSize = this[DockStyle.Right].AutoHidePortion >= 1
                    ? (int)this[DockStyle.Right].AutoHidePortion
                    : (int)(width * this[DockStyle.Right].AutoHidePortion);
                dockLeftSize = Math.Max(dockLeftSize, Const.MinGridSize);
                dockRightSize = Math.Max(dockRightSize, Const.MinGridSize);

                var diff = (dockLeftSize + dockRightSize) - (width - Const.MinGridSize);
                if (diff > 0)
                {
                    dockLeftSize -= diff / 2;
                    dockRightSize -= diff / 2;
                }

                return dockStyle == DockStyle.Left ? dockLeftSize : dockRightSize;
            }
            if (dockStyle == DockStyle.Top || dockStyle == DockStyle.Bottom)
            {
                var height = dockPanel.DockBounds.Height;
                var dockTopSize = this[DockStyle.Top].AutoHidePortion >= 1
                    ? (int)this[DockStyle.Top].AutoHidePortion
                    : (int)(height * this[DockStyle.Top].AutoHidePortion);
                var dockBottomSize = this[DockStyle.Bottom].AutoHidePortion >= 1
                    ? (int)this[DockStyle.Bottom].AutoHidePortion
                    : (int)(height * this[DockStyle.Bottom].AutoHidePortion);
                dockTopSize = Math.Max(dockTopSize, Const.MinGridSize);
                dockBottomSize = Math.Max(dockBottomSize, Const.MinGridSize);

                var diff = (dockTopSize + dockBottomSize) - (height - Const.MinGridSize);
                if (diff > 0)
                {
                    dockTopSize -= diff / 2;
                    dockBottomSize -= diff / 2;
                }

                return dockStyle == DockStyle.Top ? dockTopSize : dockBottomSize;
            }
            return 0;
        }
        public void SetDockPortion(DockStyle dockStyle, double portion)
        {
            if (portion <= 0)
                throw new ArgumentOutOfRangeException("portion");
            if (this[dockStyle].AutoHidePortion == portion)
                return;
            this[dockStyle].AutoHidePortion = portion;
            switch (dockStyle)
            {
                case DockStyle.Left:
                    if (this[DockStyle.Left].AutoHidePortion < 1 && this[DockStyle.Right].AutoHidePortion < 1)
                    {
                        if (this[DockStyle.Left].AutoHidePortion + this[DockStyle.Right].AutoHidePortion > 1)
                            this[DockStyle.Left].AutoHidePortion = 1 - this[DockStyle.Right].AutoHidePortion;
                    }
                    break;
                case DockStyle.Right:
                    if (this[DockStyle.Right].AutoHidePortion < 1 && this[DockStyle.Left].AutoHidePortion < 1)
                    {
                        if (this[DockStyle.Right].AutoHidePortion + this[DockStyle.Left].AutoHidePortion > 1)
                            this[DockStyle.Right].AutoHidePortion = 1 - this[DockStyle.Left].AutoHidePortion;
                    }
                    break;
                case DockStyle.Top:
                    if (this[DockStyle.Top].AutoHidePortion < 1 && this[DockStyle.Bottom].AutoHidePortion < 1)
                    {
                        if (this[DockStyle.Top].AutoHidePortion + this[DockStyle.Bottom].AutoHidePortion > 1)
                            this[DockStyle.Top].AutoHidePortion = 1 - this[DockStyle.Bottom].AutoHidePortion;
                    }
                    break;
                case DockStyle.Bottom:
                    if (this[DockStyle.Bottom].AutoHidePortion < 1 && this[DockStyle.Top].AutoHidePortion < 1)
                    {
                        if (this[DockStyle.Bottom].AutoHidePortion + this[DockStyle.Top].AutoHidePortion > 1)
                            this[DockStyle.Bottom].AutoHidePortion = 1 - this[DockStyle.Top].AutoHidePortion;
                    }
                    break;
            }
            dockPanel.PerformLayout();
        }
    }
}
