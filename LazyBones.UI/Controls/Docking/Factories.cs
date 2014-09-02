using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    public interface IDockGridCaptionFactory
    {
        DockGridCaptionBase CreateDockGridCaption(DockGrid dockGrid);
    }
    public interface IDockGridStripFactory
    {
        DockGridStripBase CreateDockGridStrip(DockGrid dockGrid);
    }
    public interface IAutoHideStripFactory
    {
        AutoHideStripBase CreateAutoHideStrip(DockPanel dockPanel);
    }
    public interface IDockGridFactory
    {
        DockGrid CreateDockGrid(IDockContent content, DockStyle visibleStyle);
        //DockGrid CreateDockGrid(IDockContent content, FloatWindow floatWindow);
        DockGrid CreateDockGrid(IDockContent content, DockGrid previousGrid, DockStyle dockStyle, double proportion);
        DockGrid CreateDockGrid(IDockContent content, Rectangle floatWindowBounds);
    }
    public interface IFloatWindowFactory
    {
        FloatWindow CreateFloatWindow(DockPanel dockPanel);
        FloatWindow CreateFloatWindow(DockPanel dockPanel, Rectangle bounds);
    }
    internal class DockGridFactory : IDockGridFactory
    {
        public DockGrid CreateDockGrid(IDockContent content, DockStyle visibleStyle)
        {
            return new DockGrid(content, visibleStyle);
        }
        public DockGrid CreateDockGrid(IDockContent content, Rectangle floatWindowBounds)
        {
            return new DockGrid(content, floatWindowBounds);
        }

        public DockGrid CreateDockGrid(IDockContent content, DockGrid previousGrid, DockStyle visibleStyle, double proportion)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class DockGridCaptionFactory : IDockGridCaptionFactory
    {
        public DockGridCaptionBase CreateDockGridCaption(DockGrid dockGrid)
        {
            return new DockGridCaption(dockGrid);
        }
    }
    internal class DockGridStripFactory : IDockGridStripFactory
    {
        public DockGridStripBase CreateDockGridStrip(DockGrid dockGrid)
        {
            return new DockGridStrip(dockGrid);
        }
    }
    internal class AutoHideStripFactory : IAutoHideStripFactory
    {
        public AutoHideStripBase CreateAutoHideStrip(DockPanel dockPanel)
        {
            return new AutoHideStrip(dockPanel);
        }
    }
    internal class FloatWindowFactory : IFloatWindowFactory
    {
        public FloatWindow CreateFloatWindow(DockPanel dockPanel)
        {
            return new FloatWindow(dockPanel);
        }
        public FloatWindow CreateFloatWindow(DockPanel dockPanel, Rectangle bounds)
        {
            return new FloatWindow(dockPanel, bounds);
        }
    }
}
