using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Docking
{
    /// <summary>
    /// 处理分隔栏的拖拽
    /// </summary>
    internal class SplitterDragHandler : DragHandler<ISplitterDragSource>
    {
        FormDrag dragForm;
        ISplitterDragSource splitterDragSource;
        Rectangle splitterRect;
        public SplitterDragHandler(DockPanel dockPanel)
            : base(dockPanel)
        {
        }

        public void BeginDrag(ISplitterDragSource dragSource, Rectangle splitterScreenBounds)
        {
            splitterDragSource = dragSource;
            this.splitterRect = splitterScreenBounds;

            if (!BeginDrag())
            {
                splitterDragSource = null;
                return;
            }

            dragForm = new FormDrag();
            dragForm.BackColor = Color.Black;
            dragForm.Opacity = 0.7;
            dragForm.Bounds = splitterScreenBounds;
            dragForm.ShowAndActivate();
            
            splitterDragSource.BeginDrag(splitterScreenBounds);
        }

        protected override void OnDragging()
        {
            dragForm.Bounds = GetSplitterOutlineBounds(Cursor.Position);
        }

        protected override void OnEndDrag(bool abort)
        {
            DockPanel.SuspendLayout();

            dragForm.Close();
            dragForm = null;

            if (!abort)
                splitterDragSource.MoveSplitter(GetMovingOffset(Cursor.Position));

            splitterDragSource.EndDrag();
            DockPanel.ResumeLayout(true);
        }

        int GetMovingOffset(Point ptMouse)
        {
            var rect = GetSplitterOutlineBounds(ptMouse);
            if (splitterDragSource.IsVertical)
                return rect.X - splitterRect.X;
            else
                return rect.Y - splitterRect.Y;
        }

        Rectangle GetSplitterOutlineBounds(Point ptMouse)
        {
            var rectLimit = splitterDragSource.DragLimitBounds;
            if (rectLimit.Width <= 0 || rectLimit.Height <= 0)
                return splitterRect;

            var rect = splitterRect;
            if (splitterDragSource.IsVertical)
            {
                rect.X += ptMouse.X - StartMousePosition.X;
                rect.Height = rectLimit.Height;
                if (rect.X < rectLimit.X)
                    rect.X = rectLimit.X;
                if (rect.Right > rectLimit.Right)
                    rect.X -= rect.Right - rectLimit.Right;
            }
            else
            {
                rect.Y += ptMouse.Y - StartMousePosition.Y;
                rect.Width = rectLimit.Width;
                if (rect.Y < rectLimit.Y)
                    rect.Y = rectLimit.Y;
                if (rect.Bottom > rectLimit.Bottom)
                    rect.Y -= rect.Bottom - rectLimit.Bottom;
            }
            return rect;
        }
    }    
}
