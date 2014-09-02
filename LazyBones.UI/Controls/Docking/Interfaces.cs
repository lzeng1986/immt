using System;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    //定义LazyBones.UI.Controls.Docking中用到的接口
    public interface IFocusManager
    {
        void SuspendFocus();
        void ResumeFocus();
        bool IsFocusTrackingSuspended { get; }
        IDockContent ActiveContent { get; }
        DockGrid ActiveGrid { get; }
        IDockContent ActiveDocument { get; }
        DockGrid ActiveDocumentGrid { get; }
    }
    public interface IContentFocusManager
    {
        void Activate(IDockContent content);
        void GiveUpFocus(IDockContent content);
        void AddToList(IDockContent content);
        void RemoveFromList(IDockContent content);
    }
    public interface IDockContent
    {
        DockContentHandler Handler { get; }
        void OnActivated(EventArgs e);
        void OnDeactivate(EventArgs e);
    }

    public interface INestedGridsContainer
    {
        DockStyle DockStyle { get; }
        Rectangle DisplayRectangle { get; }
        NestedGridCollection NestedGrids { get; }
        bool IsFloat { get; }
        Size Size { get; }
        bool Visible { get; }

    }

    internal interface IDragSource
    {
        Control DragControl { get; }
    }

    internal interface IDockDragSource : IDragSource
    {
        Rectangle BeginDrag(Point ptMouse);
        void EndDrag();
        bool IsDockValid(DockStyle dockStyle);
        bool CanDockTo(DockGrid grid);
        void FloatAt(Rectangle floatWindowBounds);
        void DockTo(DockGrid grid, DockStyle dockStyle, int contentIndex);
        void DockTo(DockPanel panel, DockStyle dockStyle);
    }

    internal interface ISplitterDragSource : IDragSource
    {
        void BeginDrag(Rectangle splitterScreenBounds);
        void EndDrag();
        bool IsVertical { get; }
        Rectangle DragLimitBounds { get; }
        void MoveSplitter(int offset);
    }
}
