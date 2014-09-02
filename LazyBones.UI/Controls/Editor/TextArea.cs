using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 显示文字，中间类，用于所有绘制类的数据交换，一个TextArea由多个Margin组成
    /// </summary>
    public class TextArea : Control
    {
        TextEditor textEditor;
        internal Caret Caret;
        internal TextMeasurer TextMeasurer;
        internal TextMargin TextMargin;
        internal LineNoMargin LineNoMargin;
        internal FoldMargin FoldMargin;
        internal Point MouseDownPos;
        readonly internal Document Document;
        internal MarginBase[] margins;
        internal SelectionManager SelectionManager;

        public TextArea(TextEditor textEditor)
        {
            this.textEditor = textEditor;
            Document = textEditor.Document;
            SelectionManager = new SelectionManager(this);
            SelectionManager.SelectionChanged += new EventHandler(SelectionManager_SelectionChanged);

            TextMargin = new TextMargin(this);
            LineNoMargin = new LineNoMargin(this);
            FoldMargin = new FoldMargin(this);
            margins = new MarginBase[] { TextMargin, LineNoMargin, FoldMargin };

            Caret = new Caret(this);
            Clipboard = new TextAreaClipboardHandler(this);
            TextMeasurer = new TextMeasurer(this);
            EditorOptions = Document.EditorOptions;

            OptionsChanged();

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable, true);
        }

        void SelectionManager_SelectionChanged(object sender, EventArgs e)
        {

        }
        public EditorOptions EditorOptions { get; private set; }
        public TextEditor TextEditor { get { return textEditor; } }

        public int MaxVScrollValue
        {
            get
            {
                return (Document.GetVisibleLine(Document.TotalOfLine - 1) + 1 + TextMargin.VisibleLineCount * 2 / 3) * TextMeasurer.FontHeight;
            }
        }
        Point visualLocation = Point.Empty;
        public Point VisualLocation
        {
            get { return visualLocation; }
            set
            {
                var newLocation = new Point(value.X, Math.Min(MaxVScrollValue, Math.Max(0, value.Y)));
                if (visualLocation == newLocation)
                    return;
                visualLocation = newLocation;
                textEditor.VScrollBar.Value = visualLocation.Y;
                Invalidate();
                Caret.UpdateCaretPosition();
            }
        }

        internal TextAreaClipboardHandler Clipboard { get; private set; }
        public bool AutoClearSelection { get; set; }
        public void BeginUpdate()
        {
            textEditor.BeginUpdate();
        }
        public void EndUpdate()
        {
            textEditor.EndUpdate();
        }
        public void SetDesiredColumn()
        {
            Caret.DesiredColumn = TextMeasurer.GetDrawingXPos(Caret.Line, Caret.Column) + VisualLocation.X;
        }

        public void SetCaretToDesiredColumn()
        {
            Caret.Location = TextMargin.PointToLocation(Caret.Line, Caret.DesiredColumn + VisualLocation.X);
        }

        MarginBase updateMargin = null;

        internal void Refresh(MarginBase margin)
        {
            updateMargin = margin;
            Invalidate(updateMargin.DisplayBounds);
            Update();
            updateMargin = null;
        }
        protected internal virtual bool HandleKeyPress(char ch)
        {
            return false;
        }

        protected override bool IsInputChar(char charCode)
        {
            return true;
        }

        internal bool IsReadOnlyAt(int offset)
        {
            if (Document.ReadOnly)
                return true;
            return EditorOptions.SupportReadOnlySegment && Document.TextMarkerManager.GetMarkers(offset).Any(m => m.ReadOnly);
        }

        internal bool IsReadOnlyAt(int offset, int length)
        {
            if (Document.ReadOnly)
                return true;
            return EditorOptions.SupportReadOnlySegment && Document.TextMarkerManager.GetMarkers(offset, length).Any(m => m.ReadOnly);
        }
        void ClearSelection()
        {
            if (Document.EditorOptions.SelectionMode == SelectionMode.Normal && !SelectionManager.IsEmpty)
            {
                Caret.Location = SelectionManager.Selections[0].Start;
                SelectionManager.RemoveSelectedText();
            }
        }
        public void Insert(char ch)
        {
            bool updating = textEditor.IsInUpdate;
            if (!updating)
                BeginUpdate();

            // filter out forgein whitespace chars and replace them with standard space (ASCII 32)
            if (Char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n')
            {
                ch = ' ';
            }

            Document.UndoStack.StartUndoGroup();
            ClearSelection();
            Document.Insert(Caret.Offset, ch.ToString());
            Document.UndoStack.EndUndoGroup();
            ++Caret.Column;

            if (!updating)
            {
                EndUpdate();
                UpdateLineToEnd(Caret.Line, Caret.Column);
            }
        }

        public void Insert(string str)
        {
            bool updating = textEditor.IsInUpdate;
            if (!updating)
            {
                BeginUpdate();
            }
            try
            {
                Document.UndoStack.StartUndoGroup();
                ClearSelection();

                int oldOffset = Caret.Offset;
                int oldLine = Caret.Line;
                Document.Insert(oldOffset, str);
                Caret.Location = Document.OffsetToLocation(oldOffset + str.Length);
                Document.UndoStack.EndUndoGroup();
                if (oldLine != Caret.Line)
                {
                    UpdateToEnd(oldLine);
                }
                else
                {
                    UpdateLineToEnd(Caret.Line, Caret.Column);
                }
            }
            finally
            {
                if (!updating)
                {
                    EndUpdate();
                }
            }
        }

        public void Replace(char ch)
        {
            bool updating = textEditor.IsInUpdate;
            if (!updating)
            {
                BeginUpdate();
            }
            ClearSelection();

            int lineNr = Caret.Line;
            var line = Document.GetLineByLineNo(lineNr);
            int offset = Document.LocationToOffset(Caret.Location);
            if (offset < line.Offset + line.Length)
            {
                Document.Replace(offset, 1, ch.ToString());
            }
            else
            {
                Document.Insert(offset, ch.ToString());
            }
            if (!updating)
            {
                EndUpdate();
                UpdateLineToEnd(lineNr, Caret.Column);
            }
            ++Caret.Column;
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            SimulateKeyPress(e.KeyChar);
            e.Handled = true;
        }
        public void SimulateKeyPress(char ch)
        {
            if (SelectionManager.IsEmpty && SelectionManager.ReadOnly)
                return;

            if (IsReadOnlyAt(Caret.Offset))
                return;

            if (ch < ' ')
            {
                return;
            }

            if (!hiddenMouseCursor)
            {
                if (this.ClientRectangle.Contains(PointToClient(Cursor.Position)))
                {
                    cursorHidePosition = Cursor.Position;
                    hiddenMouseCursor = true;
                    Cursor.Hide();
                }
            }
            CloseToolTip();

            BeginUpdate();
            Document.UndoStack.StartUndoGroup();
            try
            {
                // INSERT char
                if (HandleKeyPress(ch))
                    return;
                switch (Caret.Mode)
                {
                    case CaretMode.Insert:
                        Insert(ch);
                        break;
                    case CaretMode.Overwrite:
                        Replace(ch);
                        break;
                }
                int currentLineNr = Caret.Line;
                //Document.FormattingStrategy.FormatLine(this, currentLineNr, Document.PositionToOffset(Caret.Position), ch);

                EndUpdate();
            }
            finally
            {
                Document.UndoStack.EndUndoGroup();
            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            var action = textEditor.KeyActions[keyData];
            AutoClearSelection = true;
            if (action == null)
                return base.ProcessDialogKey(keyData);

            BeginUpdate();
            try
            {
                lock (Document)
                {
                    action.Execute(this);
                    if (SelectionManager.HasSelection && AutoClearSelection)
                    {
                        if (Document.EditorOptions.SelectionMode == SelectionMode.Normal)
                        {
                            SelectionManager.ClearSelection();
                        }
                    }
                }
                return true;
            }
            finally
            {
                EndUpdate();
                Caret.UpdateCaretPosition();
            }
        }

        bool hiddenMouseCursor = false;
        Point cursorHidePosition;
        internal void ShowHiddenCursor(bool forceShow)
        {
            if (hiddenMouseCursor)
            {
                if (cursorHidePosition != Cursor.Position || forceShow)
                {
                    Cursor.Show();
                    hiddenMouseCursor = false;
                }
            }
        }

        public void OptionsChanged()
        {
            TextMeasurer.OptionsChanged();
            Caret.RecreateCaret();
            Caret.UpdateCaretPosition();
            Refresh();
        }

        bool toolTipActive;
        Rectangle toolTipRectangle;

        internal void CloseToolTip()
        {
            if (toolTipActive)
            {
                toolTipActive = false;
                SetToolTip(null, -1);
            }
            ResetMouseEventArgs();
        }
        static FormToolTip formToolTip;
        static string oldToolTip;
        void SetToolTip(string text, int lineNo)
        {
            if (oldToolTip == text)
                return;

            if (formToolTip == null || formToolTip.IsDisposed)
                formToolTip = new FormToolTip { Owner = this.FindForm(), HideOnClick = true, Parent = this };

            var pos = PointToClient(Cursor.Position);
            pos.Offset(3, 3);
            formToolTip.Location = pos;
            formToolTip.Description = text;

            oldToolTip = text;
        }
        public TextLocation MouseLocation
        {
            get
            {
                var mousePos = PointToClient(Cursor.Position);
                var realMousePos = TextMargin.PointToLocation(
                    mousePos.Y - TextMargin.DisplayBounds.Y, Math.Max(0, mousePos.X - TextMargin.DisplayBounds.X)
                    );
                return Caret.ValidatePosition(realMousePos);
            }
        }

        MarginBase lastMouseInMargin;
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            foreach (var margin in margins.Where(m => m.Visible))
                margin.MouseLeave(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            foreach (var margin in margins.Where(m => m.Visible))
                margin.MouseEnter(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            TriggerMarginMouseAction(e.Location, m => m.MouseDown(e));
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            TriggerMarginMouseAction(e.Location, m => m.MouseDoubleClick(e));
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            TriggerMarginMouseAction(e.Location, m => m.MouseMove(e));
        }
        void TriggerMarginMouseAction(Point mouseLocation, Action<MarginBase> action)
        {
            for (var i = 0; i < margins.Length; i++)
            {
                var m = margins[i];
                if (m.Visible && m.DisplayBounds.Contains(mouseLocation))
                {
                    action(m);
                    break;
                }
            }
        }
        internal void RaiseMouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle clipRectangle = e.ClipRectangle;

            bool isFullRepaint = clipRectangle == this.DisplayRectangle;

            g.TextRenderingHint = this.EditorOptions.TextRenderingHint;

            if (updateMargin != null)
                updateMargin.Paint(g, updateMargin.DisplayBounds);

            if (clipRectangle.Width <= 0 || clipRectangle.Height <= 0)
                return;

            int currentXPos = 0;
            int currentYPos = 0;
            bool adjustScrollBars = false;
            foreach (var margin in margins.Where(m => m.Visible))//设置margin的边界范围
            {
                var rect = new Rectangle(currentXPos, currentYPos, margin.DisplayBounds.Width, Height - currentYPos);
                if (rect != margin.DisplayBounds)
                {
                    if (!isFullRepaint && !clipRectangle.Contains(rect))
                    {
                        Invalidate(); // do a full repaint
                    }
                    adjustScrollBars = true;
                    margin.DisplayBounds = rect;
                }
                currentXPos += margin.DisplayBounds.Width;
                if (clipRectangle.IntersectsWith(rect))
                {
                    rect.Intersect(clipRectangle);
                    margin.Paint(g, rect);
                }
            }

            // update caret position (but outside of WM_PAINT!)
            BeginInvoke((Action)Caret.UpdateCaretPosition);

            if (adjustScrollBars)
                textEditor.AdjustScrollBars();

            // we cannot update the caret position here, it's not allowed to call the caret API inside WM_PAINT
            //Caret.UpdateCaretPosition();

            base.OnPaint(e);
        }
        internal void UpdateLine(int line)
        {
            UpdateLines(0, line, line);
        }
        internal void UpdateLine(int xPos, int line)
        {
            UpdateLines(xPos, line, line);
        }
        internal void UpdateLines(int lineBegin, int lineEnd)
        {
            UpdateLines(0, lineBegin, lineEnd);
        }
        internal void UpdateLines(int columnStart, int lineBegin, int lineEnd)
        {
            InvalidateLines((int)(columnStart * TextMeasurer.WideSpaceWidth), lineBegin, lineEnd);
        }
        void InvalidateLines(int xPos, int lineBegin, int lineEnd)
        {
            lineBegin = Document.GetVisibleLine(lineBegin);
            lineEnd = Document.GetVisibleLine(lineEnd);
            int y = Document.GetVisibleLine(lineBegin) * TextMeasurer.FontHeight - VisualLocation.Y;
            y = Math.Max(0, y);
            int height = Math.Min(TextMargin.DisplayBounds.Height, (1 + lineEnd - lineBegin) * TextMeasurer.FontHeight);
            height = Math.Max(0, height);
            Invalidate(new Rectangle(xPos, y, Width, height + 3));
        }

        internal void UpdateToEnd(int lineBegin)
        {
            lineBegin = Document.GetVisibleLine(lineBegin);
            int y = Math.Max(0, (int)(lineBegin * TextMeasurer.FontHeight));
            y = Math.Max(0, y - this.visualLocation.Y);
            Invalidate(new Rectangle(0, y, Width, Height - y));
        }

        internal void UpdateLineToEnd(int lineNr, int columnStart)
        {
            UpdateLines(columnStart, lineNr, lineNr);
        }

        public event KeyEventHandler KeyEventHandler;
        public event EventHandler<ToolTipRequestEventArgs> ToolTipRequest;

        protected virtual void OnToolTipRequest(ToolTipRequestEventArgs e)
        {
            if (ToolTipRequest != null)
                ToolTipRequest(this, e);
        }
    }
}
