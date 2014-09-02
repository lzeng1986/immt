using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    abstract class InputState
    {
        public TreeList Tree { get; internal set; }

        public virtual void KeyDown(KeyEventArgs args) { }
        public virtual void MouseDown(TreeListNodeMouseEventArgs args) { }
        public virtual void MouseUp(TreeListNodeMouseEventArgs args) { }

        public virtual bool MouseMove(MouseEventArgs args)
        {
            return false;
        }
    }

    class NormalInputState : InputState
    {
        bool mouseDownFlag = false;

        public override void KeyDown(KeyEventArgs args)
        {
            if (Tree.CurrentNode == null && Tree.RootNodes.Count > 0)
                Tree.CurrentNode = Tree.RootNodes[0];
            if (Tree.CurrentNode == null)
                return;
            args.Handled = true;
            switch (args.KeyCode)
            {
                case Keys.Enter:
                    Tree.CurrentNode.IsExpanded = !Tree.CurrentNode.IsExpanded;
                    break;
                case Keys.Right:
                    Tree.CurrentNode.IsExpanded = true;
                    break;
                case Keys.Left:
                    Tree.CurrentNode.IsExpanded = false;
                    break;
                case Keys.Down:
                    Navigate(1);
                    break;
                case Keys.Up:
                    Navigate(-1);
                    break;
                case Keys.PageDown:
                    Navigate(Tree.PageRowCount);
                    break;
                case Keys.PageUp:
                    Navigate(-Tree.PageRowCount);
                    break;
                case Keys.Home:
                    if (Tree.VisibleNodes.Count > 0)
                        FocusRow(Tree.VisibleNodes.First());
                    break;
                case Keys.End:
                    if (Tree.VisibleNodes.Count > 0)
                        FocusRow(Tree.VisibleNodes.Last());
                    break;
                default:
                    args.Handled = false;
                    break;
            }
        }
        public override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            if (args.Node != null)
            {
                Tree.ItemDragMode = true;
                Tree.ItemDragStart = args.Location;

                if (args.Button == MouseButtons.Left)
                {
                    using (Tree.GetUpdateHolder())
                    {
                        Tree.CurrentNode = args.Node;
                        if (args.Node.IsSelected)
                            mouseDownFlag = true;
                        else
                        {
                            mouseDownFlag = false;
                            DoMouseOperation(args);
                        }
                    }
                }

            }
            else
            {
                Tree.ItemDragMode = false;
                MouseDownAtEmptySpace(args);
            }
        }
        public override void MouseUp(TreeListNodeMouseEventArgs args)
        {
            Tree.ItemDragMode = false;
            if (mouseDownFlag)
            {
                if (args.Button == MouseButtons.Left)
                    DoMouseOperation(args);
                else if (args.Button == MouseButtons.Right)
                    Tree.CurrentNode = args.Node;
            }
            mouseDownFlag = false;
        }
        void Navigate(int offset)
        {
            var row = Tree.CurrentNode.RowIndex + offset;
            row = Math.Max(Math.Min(row, Tree.VisibleNodes.Count - 1), 0);
            if (row != Tree.CurrentNode.RowIndex)
                FocusRow(Tree.VisibleNodes[row]);
        }
        protected virtual void MouseDownAtEmptySpace(TreeListNodeMouseEventArgs args)
        {
            Tree.ClearSelection();
        }
        protected virtual void FocusRow(TreeListNode node)
        {
            Tree.SuspendSelectionEvent = true;
            try
            {
                Tree.ClearSelection();
                Tree.CurrentNode = node;
                Tree.SelectionStart = node;
                node.IsSelected = true;
                Tree.ScrollTo(node);
            }
            finally
            {
                Tree.SuspendSelectionEvent = false;
            }
        }
        protected bool CanSelect(TreeListNode node)
        {
            if (Tree.SelectionMode == TreeSelectionMode.MultiSameParent)
            {
                return (Tree.SelectionStart == null || node.ParentNode == Tree.SelectionStart.ParentNode);
            }
            else
                return true;
        }

        protected virtual void DoMouseOperation(TreeListNodeMouseEventArgs args)
        {
            Tree.SuspendSelectionEvent = true;
            try
            {
                Tree.ClearSelection();
                if (args.Node != null)
                    args.Node.IsSelected = true;
                Tree.SelectionStart = args.Node;
            }
            finally
            {
                Tree.SuspendSelectionEvent = false;
            }
        }
    }

    class ResizeColumnState : InputState
    {
        const int MinColumnWidth = 10;
        Point initLocation;
        TreeColumn column;
        int initWidth;

        public override void KeyDown(KeyEventArgs args)
        {
            args.Handled = true;
            if (args.KeyCode == Keys.Escape)
                FinishResize();
        }
        public override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            initLocation = args.Location;
            column = args.Cell.Column;
            initWidth = column.Width;
            base.MouseDown(args);
        }
        public override void MouseUp(TreeListNodeMouseEventArgs args)
        {
            FinishResize();
        }

        void FinishResize()
        {
            Tree.ChangeInput();
        }

        public override bool MouseMove(MouseEventArgs args)
        {
            int w = initWidth + args.X - initLocation.X;
            column.Width = Math.Max(MinColumnWidth, w);
            Tree.UpdateColumnWidth(column);
            return true;
        }

    }
    class InputWithControl : NormalInputState
    {
        protected override void DoMouseOperation(TreeListNodeMouseEventArgs args)
        {
            if (Tree.SelectionMode == TreeSelectionMode.Single)
            {
                base.DoMouseOperation(args);
            }
            else if (CanSelect(args.Node))
            {
                args.Node.IsSelected = !args.Node.IsSelected;
                Tree.SelectionStart = args.Node;
            }
        }

        protected override void MouseDownAtEmptySpace(TreeListNodeMouseEventArgs args)
        {
        }
    }
    class InputWithShift : NormalInputState
    {
        protected override void FocusRow(TreeListNode node)
        {
            Tree.SuspendSelectionEvent = true;
            try
            {
                if (Tree.SelectionMode == TreeSelectionMode.Single || Tree.SelectionStart == null)
                    base.FocusRow(node);
                else if (CanSelect(node))
                {
                    SelectAllFromStart(node);
                    Tree.CurrentNode = node;
                    Tree.ScrollTo(node);
                }
            }
            finally
            {
                Tree.SuspendSelectionEvent = false;
            }
        }

        protected override void DoMouseOperation(TreeListNodeMouseEventArgs args)
        {
            if (Tree.SelectionMode == TreeSelectionMode.Single || Tree.SelectionStart == null)
            {
                base.DoMouseOperation(args);
            }
            else if (CanSelect(args.Node))
            {
                Tree.SuspendSelectionEvent = true;
                try
                {
                    SelectAllFromStart(args.Node);
                }
                finally
                {
                    Tree.SuspendSelectionEvent = false;
                }
            }
        }

        protected override void MouseDownAtEmptySpace(TreeListNodeMouseEventArgs args)
        {
        }

        void SelectAllFromStart(TreeListNode node)
        {
            Tree.ClearSelection();
            int a = Math.Min(node.RowIndex, Tree.SelectionStart.RowIndex);
            int b = Math.Max(node.RowIndex, Tree.SelectionStart.RowIndex);
            for (int i = a; i <= b; i++)
            {
                if (Tree.SelectionMode == TreeSelectionMode.Multi || Tree.VisibleNodes[i].ParentNode == node.ParentNode)
                    Tree.VisibleNodes[i].IsSelected = true;
            }
        }
    }
}
