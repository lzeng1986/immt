using System;
using System.Drawing;
using System.Windows.Forms;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    // 分隔栏
    class Splitter : Control
    {
        DockPanel dockPanel;
        public Splitter(DockPanel dockPanel)
        {
            SetStyle(ControlStyles.Selectable, false);
            this.dockPanel = dockPanel;
        }
        public override DockStyle Dock
        {
            get { return base.Dock; }
            set
            {
                SuspendLayout();
                base.Dock = value;

                switch (value)  //根据停靠状态设置控件大小及光标
                {
                    case DockStyle.Left:
                    case DockStyle.Right:
                        Width = SplitterSize;
                        Cursor = Cursors.VSplit;
                        break;
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        Height = SplitterSize;
                        Cursor = Cursors.HSplit;
                        break;
                    default:
                        Bounds = Rectangle.Empty;
                        Cursor = Cursors.Default;
                        break;
                }

                ResumeLayout();
            }
        }
        internal int SplitterSize
        {
            get { return Const.SplitterSize; }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            e.Graphics.DrawRectangle(SystemPens.ControlDark, rect);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
                StartDrag();
        }
        void StartDrag()
        {
            dockPanel.BeginDrag(Parent as ISplitterDragSource, Parent.RectangleToScreen(Bounds));
        }
        protected override void WndProc(ref Message m)
        {
            // eat the WM_MOUSEACTIVATE message
            if (m.Msg == (int)WinMsg.WM_MOUSEACTIVATE)
                return;
            base.WndProc(ref m);
        }
        protected override void OnParentChanged(System.EventArgs e)
        {
            if (Parent == null || (Parent is ISplitterDragSource))
                base.OnParentChanged(e);
            else
                throw new InvalidOperationException("父容器必须实现ISplitterDragSource接口");
        }
    }
}
