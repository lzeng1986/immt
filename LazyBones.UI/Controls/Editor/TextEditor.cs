using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.IO;

namespace LazyBones.UI.Controls.Editor
{
    public class TextEditor : Control
    {
        HorizontalRuler hRuler = null;
        VScrollBar vScrollBar = new VScrollBar();
        HScrollBar hScrollBar = new HScrollBar();
        bool disposed;
        Document document = new Document();
        internal Caret Caret;

        public TextEditor()
        {
            TextArea = new TextArea(this);
            Controls.Add(TextArea);

            Caret = new Caret(TextArea);

            vScrollBar.ValueChanged += new EventHandler(VScrollBarValueChanged);
            Controls.Add(this.vScrollBar);

            hScrollBar.ValueChanged += new EventHandler(HScrollBarValueChanged);
            Controls.Add(this.hScrollBar);

            SetStyle(ControlStyles.ContainerControl | ControlStyles.ResizeRedraw, true);

            document.TextContentChanged += DocumentTextContentChanged;
            document.DocumentChanged += AdjustScrollBarsOnDocumentChange;
            document.UpdateCommited += DocumentUpdateCommitted;

            DoHandleMousewheel = true;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public EditorOptions EditorOptions
        {
            get { return document.EditorOptions; }
            set
            {
                document.EditorOptions = value;
                OptionsChanged();
            }
        }

        Encoding encoding;
        [Browsable(true)]
        [DefaultValue("UTF8")]
        [TypeConverter(typeof(LazyBones.UI.Designers.EncodingConverter))]
        public Encoding Encoding
        {
            get { return encoding ?? EditorOptions.Encoding; }
            set { encoding = value; }
        }
        int updateLevel = 0;
        public virtual void BeginUpdate()
        {
            ++updateLevel;
        }
        public virtual void EndUpdate()
        {
            if (updateLevel > 0)
                updateLevel--;
            document.RaiseUpdateCommited();
            if (!IsInUpdate)
            {
                //TextArea.Caret.OnEndUpdate();
            }
        }
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public override string Text
        {
            get { return Document.Text; }
            set { Document.Text = value; }
        }
        [Browsable(false)]
        public bool CanUndo
        {
            get { return Document.UndoStack.CanUndo; }
        }
        [Browsable(false)]
        public bool CanRedo
        {
            get { return Document.UndoStack.CanRedo; }
        }
        string fileName;
        [Browsable(false)]
        [ReadOnly(true)]
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (fileName == value)
                    return;
                fileName = value;
                OnFileNameChanged(EventArgs.Empty);
            }
        }
        TextAreaKeyHandler keyActions = new TextAreaKeyHandler();

        internal TextAreaKeyHandler KeyActions { get { return keyActions; } }

        public void Undo()
        {
            if (document.ReadOnly)
                return;
            if (document.UndoStack.CanUndo)
            {
                BeginUpdate();
                Document.UndoStack.Undo();
                Document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.WholeTextArea));
                EndUpdate();
            }
        }

        public void Redo()
        {
            if (Document.ReadOnly)
            {
                return;
            }
            if (Document.UndoStack.CanRedo)
            {
                BeginUpdate();
                Document.UndoStack.Redo();

                Document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.WholeTextArea));
                EndUpdate();
            }
        }
        public event EventHandler ActiveTextAreaContainerChanged;
        protected virtual void OnActiveTextAreaContainerChanged(EventArgs e)
        {
            if (ActiveTextAreaContainerChanged != null)
                ActiveTextAreaContainerChanged(this, e);
        }
        public event EventHandler FileNameChanged;
        protected virtual void OnFileNameChanged(EventArgs e)
        {
            if (FileNameChanged != null)
                FileNameChanged(this, e);
        }

        public void SaveFile(string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write))
            {
                SaveFile(fs);
            }
            this.FileName = fileName;
        }

        public void SaveFile(Stream stream)
        {
            if (!stream.CanWrite)
                throw new ArgumentException("stream不可写");
            var streamWriter = new StreamWriter(stream, this.Encoding ?? Encoding.UTF8);
            foreach (var line in Document.Lines)
            {
                streamWriter.Write(Document.GetText(line.Offset, line.Length));
                if (line.DelimiterLength > 0)
                {
                    var charAfterLine = Document.GetChar(line.Offset + line.Length);
                    if (charAfterLine != '\n' && charAfterLine != '\r')
                        throw new InvalidOperationException("The document cannot be saved because it is corrupted.");
                    streamWriter.Write(EditorOptions.LineTerminator);
                }
            }
            streamWriter.Flush();
        }
        public TextArea TextArea { get; private set; }

        public SelectionManager SelectionManager
        {
            get { return TextArea.SelectionManager; }
        }

        internal Document Document
        {
            get { return document; }
        }

        public VScrollBar VScrollBar
        {
            get { return vScrollBar; }
        }
        public HScrollBar HScrollBar
        {
            get { return hScrollBar; }
        }

        public bool DoHandleMousewheel { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    Document.TextContentChanged -= DocumentTextContentChanged;
                    Document.DocumentChanged -= AdjustScrollBarsOnDocumentChange;
                    Document.UpdateCommited -= DocumentUpdateCommitted;
                    if (vScrollBar != null)
                    {
                        vScrollBar.Dispose();
                        vScrollBar = null;
                    }
                    if (hScrollBar != null)
                    {
                        hScrollBar.Dispose();
                        hScrollBar = null;
                    }
                    if (hRuler != null)
                    {
                        hRuler.Dispose();
                        hRuler = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        void DocumentTextContentChanged(object sender, EventArgs e)
        {
            // after the text content is changed abruptly, we need to validate the
            // caret position - otherwise the caret position is invalid for a short amount
            // of time, which can break client code that expects that the caret position is always valid
            Caret.ValidateCaretPos();
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            ResizeTextArea();
        }

        public void ResizeTextArea()
        {
            int y = 0;
            int h = 0;
            if (hRuler != null)
            {
                hRuler.Location = Point.Empty;
                hRuler.Width = Width - SystemInformation.HorizontalScrollBarArrowWidth;
                hRuler.Height = TextArea.TextMeasurer.FontHeight;

                y = hRuler.Bounds.Bottom;
                h = hRuler.Bounds.Height;
            }
            TextArea.Top = y;
            TextArea.Left = 0;
            TextArea.Width = Width - SystemInformation.HorizontalScrollBarArrowWidth;
            TextArea.Height = Height - SystemInformation.VerticalScrollBarArrowHeight - h;

            SetScrollBarBounds();
        }

        public void SetScrollBarBounds()
        {
            vScrollBar.Left = TextArea.Right;
            vScrollBar.Top = 0;
            vScrollBar.Width = SystemInformation.HorizontalScrollBarArrowWidth;
            vScrollBar.Height = Height - SystemInformation.VerticalScrollBarArrowHeight;

            hScrollBar.Left = 0;
            hScrollBar.Top = TextArea.Bottom;
            hScrollBar.Width = Width - SystemInformation.HorizontalScrollBarArrowWidth;
            hScrollBar.Height = SystemInformation.VerticalScrollBarArrowHeight;
        }

        bool adjustScrollBarsOnNextUpdate;
        Point scrollToPosOnNextUpdate;
        public bool IsInUpdate { get; set; }
        void AdjustScrollBarsOnDocumentChange(object sender, DocumentEventArgs e)
        {
            if (IsInUpdate == false)
            {
                AdjustScrollBarsClearCache();
                AdjustScrollBars();
            }
            else
            {
                adjustScrollBarsOnNextUpdate = true;
            }
        }

        void DocumentUpdateCommitted(object sender, EventArgs e)
        {
            if (IsInUpdate == false)
            {
                Caret.ValidateCaretPos();

                // AdjustScrollBarsOnCommittedUpdate
                if (!scrollToPosOnNextUpdate.IsEmpty)
                {
                    ScrollTo(scrollToPosOnNextUpdate.Y, scrollToPosOnNextUpdate.X);
                }
                if (adjustScrollBarsOnNextUpdate)
                {
                    AdjustScrollBarsClearCache();
                    AdjustScrollBars();
                }
            }
        }

        int[] lineLengthCache;
        const int LineLengthCacheAdditionalSize = 100;

        void AdjustScrollBarsClearCache()
        {
            if (lineLengthCache != null)
            {
                if (lineLengthCache.Length < this.Document.TotalOfLine + 2 * LineLengthCacheAdditionalSize)
                {
                    lineLengthCache = null;
                }
                else
                {
                    Array.Clear(lineLengthCache, 0, lineLengthCache.Length);
                }
            }
        }

        public void AdjustScrollBars()
        {
            adjustScrollBarsOnNextUpdate = false;
            vScrollBar.Minimum = 0;
            // number of visible lines in document (folding!)
            vScrollBar.Maximum = TextArea.MaxVScrollValue;
            int max = 0;

            int firstLine = TextArea.TextMargin.FirstVisibleLine;
            int lastLine = this.Document.GetFirstLogicalLine(TextArea.TextMargin.FirstPhysicalLine + TextArea.TextMargin.VisibleLineCount);
            if (lastLine >= this.Document.TotalOfLine)
                lastLine = this.Document.TotalOfLine - 1;

            if (lineLengthCache == null || lineLengthCache.Length <= lastLine)
            {
                lineLengthCache = new int[lastLine + LineLengthCacheAdditionalSize];
            }

            for (int lineNumber = firstLine; lineNumber <= lastLine; lineNumber++)
            {
                var lineSegment = this.Document.GetLineByLineNo(lineNumber);
                if (Document.FoldingManager.IsLineVisible(lineNumber))
                {
                    if (lineLengthCache[lineNumber] > 0)
                    {
                        max = Math.Max(max, lineLengthCache[lineNumber]);
                    }
                    else
                    {
                        int visualLength = TextArea.TextMeasurer.GetVisualColumnFast(lineSegment, lineSegment.Length);
                        lineLengthCache[lineNumber] = Math.Max(1, visualLength);
                        max = Math.Max(max, visualLength);
                    }
                }
            }
            hScrollBar.Minimum = 0;
            hScrollBar.Maximum = (Math.Max(max + 20, TextArea.TextMargin.VisibleColumnCount - 1));

            vScrollBar.LargeChange = Math.Max(0, TextArea.TextMargin.DisplayBounds.Height);
            vScrollBar.SmallChange = Math.Max(0, TextArea.TextMeasurer.FontHeight);

            hScrollBar.LargeChange = Math.Max(0, TextArea.TextMargin.VisibleColumnCount - 1);
            hScrollBar.SmallChange = Math.Max(0, (int)TextArea.TextMeasurer.SpaceWidth);
        }

        public void OptionsChanged()
        {
            TextArea.OptionsChanged();

            //if (EditorOptions..ShowHorizontalRuler)
            //{
            //    if (hRuler == null)
            //    {
            //        hRuler = new HRuler(TextArea);
            //        Controls.Add(hRuler);
            //        ResizeTextArea();
            //    }
            //    else
            //    {
            //        hRuler.Invalidate();
            //    }
            //}
            //else
            //{
            //    if (hRuler != null)
            //    {
            //        Controls.Remove(hRuler);
            //        hRuler.Dispose();
            //        hRuler = null;
            //        ResizeTextArea();
            //    }
            //}

            AdjustScrollBars();
        }

        void VScrollBarValueChanged(object sender, EventArgs e)
        {
            TextArea.VisualLocation = new Point(TextArea.VisualLocation.X, vScrollBar.Value);
            TextArea.Invalidate();
            AdjustScrollBars();
        }

        void HScrollBarValueChanged(object sender, EventArgs e)
        {
            TextArea.VisualLocation = new Point(hScrollBar.Value * TextArea.TextMeasurer.WideSpaceWidth, TextArea.VisualLocation.Y);
            TextArea.Invalidate();
        }

        //Util.MouseWheelHandler mouseWheelHandler = new Util.MouseWheelHandler();

        public void HandleMouseWheel(MouseEventArgs e)
        {
            //int scrollDistance = mouseWheelHandler.GetScrollAmount(e);
            //if (scrollDistance == 0)
            //    return;
            //if ((Control.ModifierKeys & Keys.Control) != 0 && EditorSetting.MouseWheelTextZoom)
            //{
            //    if (scrollDistance > 0)
            //    {
            //        motherTextEditorControl.Font = new Font(motherTextEditorControl.Font.Name,
            //                                                motherTextEditorControl.Font.Size + 1);
            //    }
            //    else
            //    {
            //        motherTextEditorControl.Font = new Font(motherTextEditorControl.Font.Name,
            //                                                Math.Max(6, motherTextEditorControl.Font.Size - 1));
            //    }
            //}
            //else
            //{
            //    if (EditorSetting.MouseWheelScrollDown)
            //        scrollDistance = -scrollDistance;
            //    int newValue = vScrollBar.Value + vScrollBar.SmallChange * scrollDistance;
            //    vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1, newValue));
            //}
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (DoHandleMousewheel)
                HandleMouseWheel(e);
        }

        public void ScrollToCaret()
        {
            ScrollTo(TextArea.Caret.Line, TextArea.Caret.Column);
        }

        public void ScrollTo(int line, int column)
        {
            if (IsInUpdate)
            {
                scrollToPosOnNextUpdate = new Point(column, line);
                return;
            }
            else
            {
                scrollToPosOnNextUpdate = Point.Empty;
            }

            ScrollTo(line);

            int curCharMin = (int)(this.hScrollBar.Value - this.hScrollBar.Minimum);
            int curCharMax = curCharMin + TextArea.TextMargin.VisibleColumnCount;

            int pos = TextArea.TextMeasurer.GetVisualColumn(line, column);

            if (TextArea.TextMargin.VisibleColumnCount < 0)
            {
                hScrollBar.Value = 0;
            }
            else
            {
                if (pos < curCharMin)
                {
                    hScrollBar.Value = (int)(Math.Max(0, pos - scrollMarginHeight));
                }
                else
                {
                    if (pos > curCharMax)
                    {
                        hScrollBar.Value = (int)Math.Max(0, Math.Min(hScrollBar.Maximum, (pos - TextArea.TextMargin.VisibleColumnCount + scrollMarginHeight)));
                    }
                }
            }
        }

        int scrollMarginHeight = 3;

        public void ScrollTo(int line)
        {
            line = Math.Max(0, Math.Min(Document.TotalOfLine - 1, line));
            line = Document.GetVisibleLine(line);
            int curLineMin = TextArea.TextMargin.FirstPhysicalLine;
            if (TextArea.TextMargin.LineHeightRemainder > 0)
            {
                curLineMin++;
            }

            if (line - scrollMarginHeight + 3 < curLineMin)
            {
                this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - scrollMarginHeight + 3) * TextArea.TextMeasurer.FontHeight));
                VScrollBarValueChanged(this, EventArgs.Empty);
            }
            else
            {
                int curLineMax = curLineMin + this.TextArea.TextMargin.VisibleLineCount;
                if (line + scrollMarginHeight - 1 > curLineMax)
                {
                    if (this.TextArea.TextMargin.VisibleLineCount == 1)
                    {
                        this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - scrollMarginHeight - 1) * TextArea.TextMeasurer.FontHeight));
                    }
                    else
                    {
                        this.vScrollBar.Value = Math.Min(this.vScrollBar.Maximum,
                                                         (line - this.TextArea.TextMargin.VisibleLineCount + scrollMarginHeight - 1) * TextArea.TextMeasurer.FontHeight);
                    }
                    VScrollBarValueChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Scroll so that the specified line is centered.
        /// </summary>
        /// <param name="line">Line to center view on</param>
        /// <param name="treshold">If this action would cause scrolling by less than or equal to
        /// <paramref name="treshold"/> lines in any direction, don't scroll.
        /// Use -1 to always center the view.</param>
        public void CenterViewOn(int line, int treshold)
        {
            line = Math.Max(0, Math.Min(Document.TotalOfLine - 1, line));
            // convert line to visible line:
            line = Document.GetVisibleLine(line);
            // subtract half the visible line count
            line -= TextArea.TextMargin.VisibleLineCount / 2;

            int curLineMin = TextArea.TextMargin.FirstPhysicalLine;
            if (TextArea.TextMargin.LineHeightRemainder > 0)
            {
                curLineMin++;
            }
            if (Math.Abs(curLineMin - line) > treshold)
            {
                // scroll:
                this.vScrollBar.Value = Math.Max(0, Math.Min(this.vScrollBar.Maximum, (line - scrollMarginHeight + 3) * TextArea.TextMeasurer.FontHeight));
                VScrollBarValueChanged(this, EventArgs.Empty);
            }
        }

        public void JumpTo(int line)
        {
            line = Math.Min(line, Document.TotalOfLine - 1);
            string text = Document.GetText(Document.GetLineByLineNo(line));
            JumpTo(line, text.Length - text.TrimStart().Length);
        }

        public void JumpTo(int line, int column)
        {
            TextArea.Focus();
            TextArea.SelectionManager.ClearSelection();
            TextArea.Caret.Location = new TextLocation(column, line);
            TextArea.SetDesiredColumn();
            ScrollToCaret();
        }

        public event MouseEventHandler ShowContextMenu;


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x007B)
            {
                if (ShowContextMenu != null)
                {
                    long lParam = m.LParam.ToInt64();
                    int x = unchecked((short)(lParam & 0xffff));
                    int y = unchecked((short)((lParam & 0xffff0000) >> 16));
                    if (x == -1 && y == -1)
                    {
                        Point pos = Caret.ScreenPosition;
                        ShowContextMenu(this, new MouseEventArgs(MouseButtons.None, 0, pos.X, pos.Y + TextArea.TextMeasurer.FontHeight, 0));
                    }
                    else
                    {
                        Point pos = PointToClient(new Point(x, y));
                        ShowContextMenu(this, new MouseEventArgs(MouseButtons.Right, 1, pos.X, pos.Y, 0));
                    }
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnEnter(EventArgs e)
        {
            Caret.ValidateCaretPos();
            base.OnEnter(e);
        }
    }
}
