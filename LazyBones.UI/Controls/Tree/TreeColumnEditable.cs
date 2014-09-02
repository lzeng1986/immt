using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace LazyBones.UI.Controls.Tree
{
    public abstract class TreeColumnEditable : TreeColumn
    {
        Timer timer;
        bool editReady;
        bool discardChanges;
        TreeListNode editNode;
        Pen focusPen = new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
        StringFormat textFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        protected TreeColumnEditable()
        {
            timer = new Timer(TimerCallback, null, -1, -1);
            EditMode = EditMode.Click;
            HoverInterval = 500;
        }
        void TimerCallback(object state)
        {
            if (Tree != null)
                Tree.Invoke(new Action(BeginEdit));
        }

        ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        [DefaultValue(ContentAlignment.MiddleCenter), Category("Appearance")]
        public ContentAlignment TextAlign
        {
            get { return textAlign; }
            set
            {
                textAlign = value;
                if (Tree != null)
                    Tree.FullUpdate();
            }
        }
        public override void Draw(TreeListNode node, DrawContext context)
        {
            if (context.CurrentEditorOwner.Column == this && node == Tree.CurrentNode)
                return;

            Rectangle clipRect = context.Bounds;
            Brush brush = SystemBrushes.ControlText;

            var text = GetText(node);
            var textSize = GetTextSize(text);
            var focusRect = new Rectangle(clipRect.X, clipRect.Y, textSize.Width, clipRect.Height);

            if (context.DrawSelection == DrawSelectionMode.Active)
            {
                brush = SystemBrushes.HighlightText;
                context.Graphics.FillRectangle(SystemBrushes.Highlight, focusRect);
            }
            else if (context.DrawSelection == DrawSelectionMode.InActive)
            {
                brush = SystemBrushes.ControlText;
                context.Graphics.FillRectangle(SystemBrushes.InactiveBorder, focusRect);
            }
            else if (context.DrawSelection == DrawSelectionMode.FullRowSelect)
            {
                brush = SystemBrushes.HighlightText;
            }

            if (!context.Enabled)
                brush = SystemBrushes.GrayText;

            if (context.DrawFocus)
            {
                focusRect.Width--;
                focusRect.Height--;
                context.Graphics.DrawRectangle(Pens.Gray, focusRect);
                context.Graphics.DrawRectangle(focusPen, focusRect);
            }
            Helper.FillAligment(TextAlign, textFormat);
            context.Graphics.DrawString(text, context.Font, brush, clipRect, textFormat);
        }

        Font font = Control.DefaultFont;
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        public override Size MeasureSize(TreeListNode node)
        {
            return GetTextSize(node);
        }

        protected Size GetTextSize(TreeListNode node)
        {
            return GetTextSize(GetText(node));
        }

        protected Size GetTextSize(string text)
        {
            var s = TextRenderer.MeasureText(text, Font);
            if (s.IsEmpty)
                return new Size(10, Font.Height);
            else
                return s;
        }

        protected virtual string GetText(TreeListNode node)
        {
            if (node.Value == null)
                return string.Empty;
            if (string.IsNullOrEmpty(DataMember))
                return node.Value.ToString();
            object obj = GetValue(node);
            return (obj != null) ? obj.ToString() : string.Empty;
        }

        protected virtual void SetText(TreeListNode node, string value)
        {
            SetValue(node, value);
        }

        [DefaultValue(EditMode.Click), Category("Behavior")]
        public EditMode EditMode { get; set; }
        [DefaultValue(500), Category("Behavior")]
        public int HoverInterval { get; set; }

        protected abstract void DoApplyChanges(TreeListNode node);
        protected abstract Control CreateEditor();
        protected abstract Size CalculateEditorSize(EditorContext context);

        public void SetEditorBounds(EditorContext context)
        {
            Size size = CalculateEditorSize(context);
            context.Editor.SetBounds(context.Bounds.X, context.Bounds.Y,
                Math.Min(size.Width, context.Bounds.Width), context.Bounds.Height);
        }

        protected virtual bool CanEdit(TreeListNode node)
        {
            return (node.Value != null);
        }
        public void BeginEdit()
        {
            if (EditMode != EditMode.None && Tree.CurrentNode != null && CanEdit(Tree.CurrentNode))
            {
                var args = new CancelEventArgs();
                OnEditorShowing(args);
                if (args.Cancel)
                    return;
                discardChanges = false;
                var control = CreateEditor();
                editReady = true;
                editNode = Tree.CurrentNode;
                control.Disposed += EditorDisposed;
                Tree.DisplayEditor(control, this);
            }
        }

        public void EndEdit(bool cancel)
        {
            discardChanges = cancel;
            Tree.HideEditor();
        }

        public virtual void UpdateEditor(Control control)
        {
        }
        void EditorDisposed(object sender, EventArgs e)
        {
            OnEditorHided();
            if (!discardChanges && editNode != null)
            {
                try
                {
                    DoApplyChanges(editNode);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Value is not valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            editNode = null;
        }

        public override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            switch (EditMode)
            {
                case EditMode.Click:
                    BeginEdit();
                    break;
                case EditMode.Hover:
                    if (Tree.ColumnHeaderVisible && args.Button == MouseButtons.Left && Control.ModifierKeys == Keys.None && args.Node.IsSelected)
                    {
                        timer.Change(0, HoverInterval);
                    }
                    break;
            }
        }
        public override void MouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
            if (Tree.ColumnHeaderVisible && EditMode == EditMode.DoubleClick)
            {
                args.Handled = true;
                BeginEdit();
            }
        }
        public event CancelEventHandler EditorShowing;
        protected void OnEditorShowing(CancelEventArgs args)
        {
            var handle = EditorShowing;
            if (handle != null)
                handle(this, args);
        }

        public event EventHandler EditorHided;
        protected void OnEditorHided()
        {
            var handle = EditorHided;
            if (handle != null)
                handle(this, EventArgs.Empty);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                focusPen.Dispose();
                textFormat.Dispose();
                timer.Dispose();
            }
        }
    }
    public enum EditMode
    {
        None,
        Click,
        DoubleClick,
        Hover
    }
}
