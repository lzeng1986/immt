using System;
using System.Collections.Generic;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 用于储存绘制文本的信息，这是一个中间类，用于所有信息的交换
    /// </summary>
    public class Document
    {
        TextLineManager lineManager;
        GapTextBuffer textBuffer = new GapTextBuffer();
        TextDrawInfoManager textColorManager = new TextDrawInfoManager();
        FoldingManager foldingManager;
        public Document()
        {
            lineManager = new TextLineManager(this, null);
            textMarkerManager = new TextMarkerManager(this);
            EditorOptions = new EditorOptions();
        }
        public TextDrawInfoManager TextDrawInfoManager
        {
            get { return textColorManager; }
        }
        public EditorOptions EditorOptions { get; set; }
        public FoldingManager FoldingManager
        {
            get { return foldingManager; }
        }
        TextMarkerManager textMarkerManager;
        public TextMarkerManager TextMarkerManager
        {
            get { return textMarkerManager; }
        }
        public UndoStack UndoStack { get; internal set; }
        public bool ReadOnly { get; set; }
        public string GetText(int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length < 0");
            return textBuffer.GetText(offset, length);
        }
        public string GetText(TextLine line)
        {
            return textBuffer.GetText(line.Offset, line.Length);
        }
        public string GetText(int lineNo)
        {
            return GetText(lineManager[lineNo]);
        }
        public char GetChar(int offset)
        {
            return textBuffer[offset];
        }
        public string Text
        {
            get { return textBuffer.Text; }
            set { textBuffer.Text = value; }
        }

        public int TotalOfLine
        {
            get { return lineManager.TotalOfLine; }
        }
        public int Length
        {
            get { return textBuffer.Length; }
        }

        public TextLine GetLineByLineNo(int lineNo)
        {
            return lineManager[lineNo];
        }
        public IList<TextLine> Lines
        {
            get { return lineManager.Lines; }
        }
        public int FindFirstNonWSChar(int offset)//查找当前位置的下一个非空白字符
        {
            while (offset < textBuffer.Length && Char.IsWhiteSpace(textBuffer[offset]))
                ++offset;
            return offset;
        }
        public int FindWordStart(int offset)//查找单词起始位置
        {
            var lineOffset = lineManager.GetLineByOffset(offset).Offset;
            while (offset > lineOffset && textBuffer[offset].IsLetterDigitOrUnderscore())
                --offset;
            return offset;
        }
        public int FindWordEnd(int offset)//查找单词结束位置
        {
            var line = lineManager.GetLineByOffset(offset);
            var endPos = line.Offset + line.Length;
            while (offset < endPos && textBuffer[offset].IsLetterDigitOrUnderscore())
                ++offset;
            return offset;
        }

        public int FindNextWordStart(int offset)//查找下一个单词起始位置，如果当前位置为空白，则直接查找下一单词位置
        {
            var line = lineManager.GetLineByOffset(offset);
            var endPos = line.Offset + line.Length;
            var type = textBuffer[offset].GetCharType();
            while (offset < endPos && textBuffer[offset].GetCharType() == type)//到单词的结束位置
                ++offset;
            while (offset < endPos && char.IsWhiteSpace(textBuffer[offset]))//跳过空白字符，到达下一个单词的开始位置
                ++offset;
            return offset;
        }
        public int FindNext(int offset, char ch)
        {
            var line = lineManager.GetLineByOffset(offset);
            var endPos = line.Offset + line.Length;
            while (offset < endPos && textBuffer[offset] != ch)
                ++offset;
            return offset;
        }
        public int FindPrevWordStart(int offset)
        {
            if (offset == 0)
                return 0;
            var line = lineManager.GetLineByOffset(offset);
            CharType t = textBuffer[offset].GetCharType();
            while (offset > line.Offset && textBuffer[offset - 1].GetCharType() == t)
                --offset;

            // if we were in whitespace, and now we're at the end of a word or operator, go back to the beginning of it
            if (t == CharType.WhiteSpace && offset > line.Offset)
            {
                t = textBuffer[offset - 1].GetCharType();
                while (offset > line.Offset && textBuffer[offset - 1].GetCharType() == t)
                {
                    --offset;
                }
            }
            return offset;
        }
        public int LocationToOffset(TextLocation location)
        {
            if (location.Line >= TotalOfLine)
                return 0;
            var line = GetLineByLineNo(location.Line);
            return Math.Min(line.Offset + location.Column, textBuffer.Length);
        }

        public TextLocation OffsetToLocation(int offset)
        {
            var line = lineManager.GetLineByOffset(offset);
            return new TextLocation(offset - line.Offset, line.LineNo);
        }
        public int OffsetToLineNo(int offset)
        {
            return lineManager.OffsetToLineNo(offset);
        }
        public int GetFirstLogicalLine(int visibleLineNo)
        {
            return lineManager.GetFirstLogicalLine(visibleLineNo);
        }
        public int GetLastLogicalLine(int visibleLineNo)
        {
            return lineManager.GetLastLogicalLine(visibleLineNo);
        }
        public int GetVisibleLine(int logicalLineNo)
        {
            return lineManager.GetVisibleLine(logicalLineNo);
        }
        public void Insert(int offset, string text)
        {
            if (ReadOnly)
                return;
            OnDocumentChanging(new DocumentEventArgs(this, offset, -1, text));

            textBuffer.Insert(offset, text);
            lineManager.Insert(offset, text);

            UndoStack.Push(new UndoInsert(this, offset, text));

            OnDocumentChanged(new DocumentEventArgs(this, offset, -1, text));
        }
        public void Remove(int offset, int length)
        {
            if (ReadOnly)
            {
                return;
            }
            OnDocumentChanging(new DocumentEventArgs(this, offset, length));
            UndoStack.Push(new UndoDelete(this, offset, GetText(offset, length)));

            textBuffer.Remove(offset, length);
            lineManager.Remove(offset, length);

            OnDocumentChanged(new DocumentEventArgs(this, offset, length));
        }

        public void Replace(int offset, int length, string text)
        {
            if (ReadOnly)
            {
                return;
            }
            OnDocumentChanging(new DocumentEventArgs(this, offset, length, text));
            UndoStack.Push(new UndoableReplace(this, offset, GetText(offset, length), text));

            textBuffer.Replace(offset, length, text);
            lineManager.Replace(offset, length, text);

            OnDocumentChanged(new DocumentEventArgs(this, offset, length, text));
        }
        public bool IsLineEmpty(int lineNo)
        {
            return IsLineEmpty(lineManager[lineNo]);
        }
        public bool IsLineEmpty(TextLine line)
        {
            for (var i = line.Offset; i < line.Offset + line.Length; i++)
            {
                if (!Char.IsWhiteSpace(textBuffer[i]))
                    return false;
            }
            return true;
        }
        public void UpdateSegmentListOnDocumentChange<T>(List<T> list, DocumentEventArgs e)
            where T : ISegment
        {
            var removedCharacters = e.Length > 0 ? e.Length : 0;
            var insertedCharacters = e.Text != null ? e.Text.Length : 0;
            for (int i = 0; i < list.Count; )
            {
                ISegment s = list[i];
                int start = s.Offset;
                int end = s.Offset + s.Length;

                if (e.Offset <= start)
                {
                    start -= removedCharacters;
                    if (start < e.Offset)
                        start = e.Offset;
                }
                if (e.Offset < end)
                {
                    end -= removedCharacters;
                    if (end < e.Offset)
                        end = e.Offset;
                }

                if (start == end)
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (e.Offset <= start)
                    start += insertedCharacters;
                if (e.Offset < end)
                    end += insertedCharacters;

                s.Offset = start;
                s.Length = end - start;
                ++i;
            }
        }
        List<TextAreaUpdater> updateQueue = new List<TextAreaUpdater>();
        public List<TextAreaUpdater> UpdateQueue
        {
            get { return updateQueue; }
        }
        public void RequestUpdate(TextAreaUpdater update)
        {
            if (updateQueue.Count == 1 && updateQueue[0].UpdateType == TextAreaUpdateType.WholeTextArea)
            {
                // if we're going to update the whole text area, we don't need to store detail updates
                return;
            }
            if (update.UpdateType == TextAreaUpdateType.WholeTextArea)
            {
                // if we're going to update the whole text area, we don't need to store detail updates
                updateQueue.Clear();
            }
            updateQueue.Add(update);
        }
        public event EventHandler UpdateCommited;
        public void RaiseUpdateCommited()
        {
            if (UpdateCommited != null)
                UpdateCommited(this, EventArgs.Empty);
        }
        public event EventHandler TextContentChanged;
        void OnTextContentChanged(EventArgs e)
        {
            if (TextContentChanged != null)
                TextContentChanged(this, e);
        }
        public event EventHandler<DocumentEventArgs> DocumentChanging;
        void OnDocumentChanging(DocumentEventArgs e)
        {
            if (DocumentChanging != null)
                DocumentChanging(this, e);
        }
        public event EventHandler<DocumentEventArgs> DocumentChanged;
        void OnDocumentChanged(DocumentEventArgs e)
        {
            if (DocumentChanged != null)
                DocumentChanged(this, e);
        }
    }
}
