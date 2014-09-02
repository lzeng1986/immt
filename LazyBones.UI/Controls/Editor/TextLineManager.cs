using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.UI.Controls.Editor
{
    // 管理文本行
    public class TextLineManager
    {
        TextLineTree lineTree = new TextLineTree();
        Document document;
        IHighlighter highlighter;
        public TextLineManager(Document document, IHighlighter highlighter)
        {
            this.document = document;
            this.highlighter = highlighter;
        }
        public int TotalOfLine
        {
            get { return lineTree.Count; }
        }
        public TextLine this[int lineNo]
        {
            get { return lineTree[lineNo]; }
        }
        public IList<TextLine> Lines
        {
            get { return lineTree; }
        }
        public TextLine GetLineByOffset(int offset)
        {
            return lineTree.GetNodebyOffset(offset).Val;
        }
        public int OffsetToLineNo(int offset)
        {
            return lineTree.GetNodebyOffset(offset).Val.LineNo;
        }
        public void Insert(int offset, string text)
        {
            Replace(offset, 0, text);
        }
        public void Remove(int offset, int length)
        {
            Replace(offset, length, string.Empty);
        }
        public void Replace(int offset, int length, string text)
        {
            var oldTotalLines = TotalOfLine;
            var lineStart = OffsetToLineNo(offset);
            var deleter = new LazyDeleter();
            RemoveInternal(deleter, offset, length);
            var totalLinesAfterRemoving = TotalOfLine;
            if (!string.IsNullOrEmpty(text))
            {
                InsertInternal(offset, text);
            }
            RunHighlighter(lineStart, 1 + Math.Max(0, TotalOfLine - totalLinesAfterRemoving));

            if (deleter.RemovedLines != null)
            {
                deleter.RemovedLines.ForEach(l => OnLineDeleted(new TextLineEventArgs(document, l)));
            }
            deleter.RaiseEvents();
            if (TotalOfLine != oldTotalLines)
                OnLineCountChanged(new LineCountChangedEventArgs(document, lineStart, TotalOfLine - oldTotalLines));
        }
        void RemoveInternal(LazyDeleter deleter, int offset, int length)
        {
            if (length <= 0)
                return;
            var node = lineTree.GetNodebyOffset(offset);
            var startLine = node.Val;
            int startLineOffset = startLine.Offset;
            if (offset + length < startLineOffset + startLine.Length)
            {
                startLine.Remove(deleter, offset - startLineOffset, length);
                SetLineLength(startLine, startLine.Length - length);
                return;
            }
            // merge startSegment with another line segment because startSegment's delimiter was deleted
            // possibly remove lines in between if multiple delimiters were deleted
            int charactersRemovedInStartLine = startLineOffset + startLine.Length - offset;
            startLine.Remove(deleter, offset - startLineOffset, charactersRemovedInStartLine);


            var endLine = GetLineByOffset(offset + length);
            if (endLine == startLine)
            {
                // special case: we are removing a part of the last line up to the
                // end of the document
                SetLineLength(startLine, startLine.Length - length);
                return;
            }
            var endSegmentOffset = endLine.Offset;
            var charactersLeftInEndLine = endSegmentOffset + endLine.Length - (offset + length);
            endLine.Remove(deleter, 0, endLine.Length - charactersLeftInEndLine);
            startLine.MergedWith(endLine, offset - startLineOffset);
            SetLineLength(startLine, startLine.Length - charactersRemovedInStartLine + charactersLeftInEndLine);
            startLine.DelimiterLength = endLine.DelimiterLength;
            // remove all segments between startSegment (excl.) and endSegment (incl.)
            do
            {
                node = node.Next();
                lineTree.Remove(node.Val);
                node.Val.Delete(deleter);
            } while (node.Val != endLine);
        }
        void InsertInternal(int offset, string text)
        {
            var line = lineTree.GetNodebyOffset(offset).Val;
            var delimiter = NextDelimiter(text, 0);
            if (delimiter == null)
            {
                line.Insert(offset - line.Offset, text.Length);
                line.Length = line.Length + text.Length;
                return;
            }
            var firstLine = line;
            firstLine.Insert(offset - line.Offset, delimiter.Offset);
            int lastDelimiterEnd = 0;
            while (delimiter != null)
            {
                // split line segment at line delimiter
                //int lineBreakOffset = offset + delimiter.Offset + delimiter.Length;
                //int segmentOffset = line.Offset;
                //int lengthAfterInsertionPos = segmentOffset + line.TotalLength - (offset + lastDelimiterEnd);
                //lineCollection.SetSegmentLength(line, lineBreakOffset - segmentOffset);
                //LineSegment newSegment = lineTree.Insert(lengthAfterInsertionPos,line);//.InsertSegmentAfter(line, lengthAfterInsertionPos);
                //line.DelimiterLength = delimiter.Length;

                //line = newSegment;
                //lastDelimiterEnd = delimiter.Offset + delimiter.Length;

                delimiter = NextDelimiter(text, lastDelimiterEnd);
            }
            firstLine.SplitTo(line);
            // insert rest after last delimiter
            if (lastDelimiterEnd != text.Length)
            {
                //line.InsertedLinePart(0, text.Length - lastDelimiterEnd);

                //SetSegmentLength(line, line.TotalLength + text.Length - lastDelimiterEnd);
            }
        }
        void RunHighlighter(int startLineNo, int lineCount)
        {
            if (highlighter != null)
            {
                var markLines = new List<TextLine>();
                var startLine = lineTree.GetNode(startLineNo);
                
                
                for (int i = 0; i < lineCount; i++)
                {
                    //markLines.Add(Current);
                }
                highlighter.MarkTokens(document, markLines);
            }
        }
        void SetLineLength(TextLine line, int length)
        {
            var delta = length - line.Length;
            if (delta != 0)
            {
                lineTree.SetLineLength(line, length);
                OnLineLengthChanged(new LineLengthChangedEventArgs(document, line, delta));
            }
        }
        //获取显示的行
        public int GetVisibleLine(int logicalLineNo)
        {
            if (!document.EditorOptions.EnableFolding)
                return logicalLineNo;

            int visibleLine = 0;
            int foldEnd = 0;
            var foldings = document.FoldingManager.TopLevelFoldedFoldings;
            foreach (var fm in foldings.TakeWhile(f => f.Start.Line < logicalLineNo))
            {
                visibleLine += fm.Start.Line - foldEnd;
                foldEnd = fm.End.Line;
            }
            visibleLine += logicalLineNo - foldEnd;
            return visibleLine;
        }

        public int GetFirstLogicalLine(int visibleLineNo)
        {
            if (!document.EditorOptions.EnableFolding)
                return visibleLineNo;
            int v = 0;
            int lastFoldEndLine = 0;
            var foldings = document.FoldingManager.TopLevelFoldedFoldings;
            foreach (var fm in foldings)
            {
                if (v + fm.Start.Line - lastFoldEndLine >= visibleLineNo)
                {
                    break;
                }
                v += fm.Start.Line - lastFoldEndLine;
                lastFoldEndLine = fm.End.Line;
            }
            return lastFoldEndLine + visibleLineNo - v;
        }

        public int GetLastLogicalLine(int visibleLineNo)
        {
            return GetFirstLogicalLine(visibleLineNo + 1) - 1;
        }
        class DelimiterLine
        {
            public int Offset;
            public int Length;
        }
        DelimiterLine delimiterLine = new DelimiterLine();
        DelimiterLine NextDelimiter(string text, int offset)
        {
            for (int i = offset; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    delimiterLine.Offset = i;
                    delimiterLine.Length = 1;
                    return delimiterLine;
                }
                if (text[i] == '\r')
                {
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        delimiterLine.Offset = i;
                        delimiterLine.Length = 2;
                        return delimiterLine;
                    }
                }
            }
            return null;
        }
        public event EventHandler<LineCountChangedEventArgs> LineCountChanged;
        protected virtual void OnLineCountChanged(LineCountChangedEventArgs e)
        {
            if (LineCountChanged != null)
                LineCountChanged(this, e);
        }
        public event EventHandler<LineLengthChangedEventArgs> LineLengthChanged;
        protected virtual void OnLineLengthChanged(LineLengthChangedEventArgs e)
        {
            if (LineLengthChanged != null)
                LineLengthChanged(this, e);
        }
        public event EventHandler<TextLineEventArgs> LineDeleted;
        protected virtual void OnLineDeleted(TextLineEventArgs e)
        {
            if (LineDeleted != null)
                LineDeleted(this, e);
        }
    }
}
