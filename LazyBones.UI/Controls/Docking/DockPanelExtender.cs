using System.Drawing;
using System.Windows.Forms;
using System;

namespace LazyBones.UI.Controls.Docking
{
    public sealed class DockPanelExtender
    {
        public DockPanelExtender()
        {
            NewDockGridCaption = grid => new DockGridCaption(grid);
            NewDockGridStrip = grid => new DockGridStrip(grid);
            NewFloatWindow = (panel, bound) => new FloatWindow(panel, bound);
            NewDockGrid = (content, style) => new DockGrid(content, style);
        }
        public Func<DockGrid, DockGridCaptionBase> NewDockGridCaption { get; set; }
        public Func<DockGrid, DockGridStripBase> NewDockGridStrip { get; set; }
        public Func<DockPanel, Rectangle, FloatWindow> NewFloatWindow { get; set; }
        public Func<IDockContent, DockGridStyle, DockGrid> NewDockGrid { get; set; }
    }
    public class DockGridStyle
    {
        public DockGrid PreviousGrid { get; set; }
        public DockStyle VisibleStyle { get; set; }
        public double Proportion { get; set; }
        public Rectangle FloatBounds { get; set; }
        public DockGridStyle()
        {
            Proportion = 0.5;
        }
    }
}
