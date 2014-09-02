using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 用于管理文档中选择的部分
    /// </summary>
    public class SelectionManager : IDisposable
    {
        List<Selection> selections = new List<Selection>();
        Document document;
        TextArea textArea;
        internal SelectFrom SelectFrom = new SelectFrom();
        public SelectionManager(TextArea textArea)
        {
            this.document = textArea.Document;
            this.textArea = textArea;
            document.DocumentChanged += document_DocumentChanged;
        }

        void document_DocumentChanged(object sender, DocumentEventArgs e)
        {
            if (e.Text == null)
            {
                Remove(e.Offset, e.Length);
            }
            else
            {
                if (e.Length < 0)
                    Insert(e.Offset, e.Text);
                else
                    Replace(e.Offset, e.Length, e.Text);
            }
        }
        public bool IsEmpty
        {
            get { return selections.Count == 0; }
        }
        public bool HasSelection
        {
            get { return selections.Count > 0; }
        }
        public bool IsSelectedAt(int offset)
        {
            return selections.Any(s => s.Contain(offset));
        }
        public Selection GetSelection(int offset)
        {
            return selections.FirstOrDefault(s => s.Contain(offset));
        }
        public IList<Selection> Selections
        {
            get { return selections.AsReadOnly(); }
        }
        internal TextLocation SelectionStart;
        public string SelectedText
        {
            get
            {
                var builder = new StringBuilder();
                selections.ForEach(s => builder.Append(s.Text));
                return builder.ToString();
            }
        }
        public void SetSelection(TextLocation start, TextLocation end)
        {

            SetSelection(new Selection(document, start, end));
        }
        public void SetSelection(Selection selection)
        {
            if (selection != null)
            {
                if (selections.Count == 1 && selection == selections[0])
                    return;
                ClearWithoutUpdate();
                selections.Add(selection);
                document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.BetweenLines, selection.Start.Line, selection.End.Line));
                document.RaiseUpdateCommited();
                OnSelectionChanged(EventArgs.Empty);
            }
            else
            {
                ClearSelection();
            }
        }
        //扩展选择文本，光标位置从oldPosition扩展至newPosition
        public void ExtendSelectionTo(TextLocation position)
        {
            if (IsEmpty)//没有选择任何文本
                return;
            var selection = selections[0];
            if (position > SelectionStart)
            {
                SetSelection(SelectionStart, position);
            }
            else
            {
                SetSelection(position, selection.End);
            }

            document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.BetweenLines, selections[0].Start.Line, selections[0].End.Line));
            document.RaiseUpdateCommited();
            OnSelectionChanged(EventArgs.Empty);
        }

        public TextLocation NextValidPosition(int line)
        {
            if (line < document.TotalOfLine - 1)
                return new TextLocation(0, line + 1);
            else
                return new TextLocation(document.GetLineByLineNo(document.TotalOfLine - 1).Length + 1, line);
        }

        void ClearWithoutUpdate()
        {
            foreach (var s in selections)
            {
                document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.BetweenLines, s.Start.Line, s.End.Line));
                OnSelectionChanged(EventArgs.Empty);
            }
            selections.Clear();
        }
        public void ClearSelection()
        {
            var mousepos = textArea.MouseDownPos;
            // this is the most logical place to reset selection starting
            // positions because it is always called before a new selection
            SelectFrom.First = SelectFrom.Where;
            var newSelectionStart = textArea.TextMargin.PointToLocation(mousepos - textArea.TextMargin.DisplayBounds.Size);
            if (SelectFrom.Where == WhereFrom.LineNo)
            {
                newSelectionStart.Line = 0;
                //				selectionStart.Y = -1;
            }
            if (newSelectionStart.Line >= document.TotalOfLine)
            {
                newSelectionStart.Line = document.TotalOfLine - 1;
                newSelectionStart.Column = document.GetLineByLineNo(document.TotalOfLine - 1).Length;
            }
            this.SelectionStart = newSelectionStart;

            ClearWithoutUpdate();
            document.RaiseUpdateCommited();
        }

        public void RemoveSelectedText()
        {
            if (ReadOnly)
            {
                ClearSelection();
                return;
            }
            List<int> lines = new List<int>();
            int offset = -1;
            bool oneLine = true;
            //			PriorityQueue queue = new PriorityQueue();
            foreach (var s in selections)
            {
                //				ISelection s = ((ISelection)queue.Remove());
                if (oneLine)
                {
                    int lineBegin = s.Start.Line;
                    if (lineBegin != s.End.Line)
                    {
                        oneLine = false;
                    }
                    else
                    {
                        lines.Add(lineBegin);
                    }
                }
                offset = s.StartOffset;
                document.Remove(s.StartOffset, s.Length);

                //				queue.Insert(-s.Offset, s);
            }
            ClearSelection();
            if (offset >= 0)
            {
                //             TODO:
                //				document.Caret.Offset = offset;
            }
            if (offset != -1)
            {
                if (oneLine)
                {
                    foreach (int i in lines)
                    {
                        document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.SingleLine, i));
                    }
                }
                else
                {
                    document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.WholeTextArea));
                }
                //document.CommitUpdate();
            }
        }

        internal void Insert(int offset, string text)
        {
            foreach (var selection in selections)
            {
                if (selection.StartOffset > offset)
                {
                    //selection.StartOffset += text.Length;
                }
                else if (selection.StartOffset + selection.Length > offset)
                {
                    //selection.Length += text.Length;
                }
            }
        }

        internal void Remove(int offset, int length)
        {
            //			foreach (ISelection selection in selectionCollection) {
            //				if (selection.Offset > offset) {
            //					selection.Offset -= length;
            //				} else if (selection.Offset + selection.Length > offset) {
            //					selection.Length -= length;
            //				}
            //			}
        }

        internal void Replace(int offset, int length, string text)
        {
            //			foreach (ISelection selection in selectionCollection) {
            //				if (selection.Offset > offset) {
            //					selection.Offset = selection.Offset - length + text.Length;
            //				} else if (selection.Offset + selection.Length > offset) {
            //					selection.Length = selection.Length - length + text.Length;
            //				}
            //			}
        }
        public bool ReadOnly
        {
            get { return document.ReadOnly || selections.Exists(s => s.ReadOnly); }
        }

        public ColumnRange GetSelectionInLine(int lineNo)
        {
            foreach (var selection in selections)
            {
                int startLine = selection.Start.Line;
                int endLine = selection.End.Line;
                if (startLine < lineNo && lineNo < endLine)
                {
                    return ColumnRange.WholeColumn;
                }

                if (startLine == lineNo)
                {
                    var line = document.GetLineByLineNo(startLine);
                    int startColumn = selection.Start.Column;
                    int endColumn = endLine == lineNo ? selection.End.Column : line.Length + 1;
                    return new ColumnRange(startColumn, endColumn);
                }

                if (endLine == lineNo)
                {
                    int endColumn = selection.End.Column;
                    return new ColumnRange(0, endColumn);
                }
            }

            return ColumnRange.Empty;
        }
        public void Dispose()
        {
            if (this.document != null)
            {
                document.DocumentChanged -= document_DocumentChanged;
                this.document = null;
            }
        }
        public void RaiseSelectionChanged()
        {
            OnSelectionChanged(EventArgs.Empty);
        }
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }
        public event EventHandler SelectionChanged;

    }
    internal class SelectFrom
    {
        public WhereFrom Where = WhereFrom.None; // last selection initiator
        public WhereFrom First = WhereFrom.None; // first selection initiator
    }

    // selection initiated from type...
    internal enum WhereFrom
    {
        None = 0,
        LineNo = 1,
        TextArea = 2
    }
}
