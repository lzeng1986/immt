using System.Windows.Forms;
using LazyBones.Win32;
using System.Drawing;

namespace LazyBones.UI.Controls.Docking
{
    internal abstract class DragHandler<TDragSource> : NativeWindow, IMessageFilter
        where TDragSource : IDragSource
    {
        protected DragHandler(DockPanel dockPanel)
        {
            DockPanel = dockPanel;
        }
        public DockPanel DockPanel { get; private set; }
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == (int)WinMsg.WM_MOUSEMOVE)
                OnDragging();
            else if (m.Msg == (int)WinMsg.WM_LBUTTONUP)
                EndDrag(false);
            else if (m.Msg == (int)WinMsg.WM_CAPTURECHANGED)
                EndDrag(true);
            else if (m.Msg == (int)WinMsg.WM_KEYDOWN && (int)m.WParam == (int)Keys.Escape)
                EndDrag(true);

            if ((m.Msg == (int)WinMsg.WM_KEYDOWN || m.Msg == (int)WinMsg.WM_KEYUP) &&
                    ((int)m.WParam == (int)Keys.ControlKey || (int)m.WParam == (int)Keys.ShiftKey))
                OnDragging();

            return OnPreFilterMessage(ref m);
        }
        protected sealed override void WndProc(ref Message m)//类似消息钩子的功能，但只会相应本窗体消息
        {
            if (m.Msg == (int)WinMsg.WM_CANCELMODE || m.Msg == (int)WinMsg.WM_CAPTURECHANGED)
                EndDrag(true);
            base.WndProc(ref m);
        }
        internal protected TDragSource DragSource { get; set; }
        protected Point StartMousePosition { get; private set; }
        protected Control DragControl { get { return DragSource == null ? null : DragSource.DragControl; } }
        protected bool BeginDrag()
        {
            if (DragControl == null)
                return false;

            StartMousePosition = Cursor.Position;

            if (!User32.DragDetect(DragControl.Handle, StartMousePosition))
            {
                return false;
            }

            DragControl.FindForm().Capture = true;
            AssignHandle(DragControl.FindForm().Handle);
            Application.AddMessageFilter(this);
            return true;
        }
        void EndDrag(bool abort)
        {
            Application.RemoveMessageFilter(this);
            ReleaseHandle();
            DragControl.FindForm().Capture = false;

            OnEndDrag(abort);
        }
        protected virtual bool OnPreFilterMessage(ref Message m)
        {
            return false;
        }
        protected abstract void OnDragging();
        protected abstract void OnEndDrag(bool abort);
    }
}
