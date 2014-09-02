using System;
using System.Drawing;
using System.Windows.Forms;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.TextEditor
{
    abstract class MarginBase : IDisposable
    {
        protected readonly TextArea textArea;
        protected MarginBase(TextArea textArea)
        {
            ParamGuard.NotNull(textArea, "textArea");
            this.textArea = textArea;
            Cursor = Cursors.Default;
            DisplayBounds = Rectangle.Empty;
        }
        public Rectangle DisplayBounds { get; set; }
        public TextArea TextArea
        {
            get { return textArea; }
        }
        public virtual Cursor Cursor { get; set; }
        public virtual bool Visible
        {
            get { return true; }
        }
        public virtual void Dispose() { }
        public virtual void Paint(Graphics g, Rectangle rect) { }
        public virtual void MouseDown(MouseEventArgs e) { }
        public virtual void MouseMove(MouseEventArgs e) { }
        public virtual void MouseDoubleClick(MouseEventArgs e) { }
        public virtual void MouseEnter(EventArgs e)
        {
            textArea.Cursor = Cursor;
        }
        public virtual void MouseLeave(EventArgs e) { }
    }
}
