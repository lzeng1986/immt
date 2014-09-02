using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    public abstract class NodeCellEditable : NodeCell
    {
        bool editFlag;
        bool discardChanges;
        public bool EditEnabled { get; set; }
        protected abstract void DoApplyChanges(TreeListNode node);

        protected abstract Control CreateEditor();

        public void SetEditorBounds(EditorContext context)
        {
            Size size = CalculateEditorSize(context);
            context.Editor.SetBounds(context.Bounds.X, context.Bounds.Y,
                Math.Min(size.Width, context.Bounds.Width), context.Bounds.Height);
        }

        protected abstract Size CalculateEditorSize(EditorContext context);

        protected virtual bool CanEdit(TreeListNode node)
        {
            return (node.Tag != null);
        }
        public void BeginEdit()
        {
            if (EditEnabled && Tree.CurrentNode != null && CanEdit(Tree.CurrentNode))
            {
                var args = new CancelEventArgs();
                OnEditorShowing(args);
                if (args.Cancel)
                    return;
                discardChanges = false;
                var control = CreateEditor();
                //editNode = Parent.CurrentNode;
                control.Disposed += EditorDisposed;
                //Tree.DisplayEditor(control, this);
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
            //if (!discardChanges && editNode != null)
            //    ApplyChanges(editNode);
            //editNode = null;
        }

        void ApplyChanges(TreeListNode node)
        {
            try
            {
                DoApplyChanges(node);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Value is not valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
    }
    public struct EditorContext
    {
        public TreeListNode CurrentNode { get; set; }
        public Control Editor { get; set; }
        public NodeCell Owner { get; set; }
        public Rectangle Bounds { get; set; }
    }
}
