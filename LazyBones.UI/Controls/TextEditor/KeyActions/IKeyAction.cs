using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.TextEditor
{
    public interface IKeyAction
    {
        Keys[] Keys { get; set; }
        void Execute(TextArea textArea);
    }
    public abstract class KeyActionBase : IKeyAction
    {
        public Keys[] Keys { get; set; }

        public abstract void Execute(TextArea textArea);
    }
    class Cut : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            if (textArea.Document.ReadOnly)
                return;
            textArea.Clipboard.Cut();
        }
    }
    class Copy : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            textArea.AutoClearSelection = false;
            textArea.Clipboard.Copy();
        }
    }
    class Paste : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            if (textArea.Document.ReadOnly)
                return;
            textArea.Clipboard.Paste();
        }
    }
    class CaretLeft : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var position = textArea.Caret.Location;
            var foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithEnd(position.Column);
            var justBeforeCaret = foldings.FirstOrDefault(fm => fm.End.Column == position.Column);

            if (justBeforeCaret != null)
            {
                position = justBeforeCaret.Start;
            }
            else
            {
                if (position.Column > 0)
                {
                    --position.Column;
                }
                else if (position.Line > 0)
                {
                    var lineAbove = textArea.Document.GetLineByLineNo(position.Line - 1);
                    position.Line--;
                    position.Column = lineAbove.Length;
                }
            }

            textArea.Caret.Location = position;
            textArea.SetDesiredColumn();
        }
    }

    class CaretRight : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var curLine = textArea.Document.GetLineByLineNo(textArea.Caret.Line);
            var position = textArea.Caret.Location;
            var foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(position.Line);
            var justBehindCaret = foldings.FirstOrDefault(f => f.Start.Column == position.Column);

            if (justBehindCaret != null)
            {
                position = justBehindCaret.End;
            }
            else
            { // no folding is interesting
                if (position.Column < curLine.Length)
                {
                    ++position.Column;
                }
                else if (position.Line + 1 < textArea.Document.TotalOfLine)
                {
                    ++position.Line;
                    position.Column = 0;
                }
            }
            textArea.Caret.Location = position;
            textArea.SetDesiredColumn();
        }
    }

    class CaretUp : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var position = textArea.Caret.Location;
            int lineNr = position.Line;
            int visualLine = textArea.Document.GetVisibleLine(lineNr);
            if (visualLine > 0)
            {
                Point pos = new Point(textArea.TextMeasurer.GetDrawingXPos(lineNr, position.Column),
                                      textArea.TextMargin.DisplayBounds.Y + (visualLine - 1) * textArea.TextMeasurer.FontHeight - textArea.TextMargin.TextArea.VisualLocation.Y);
                textArea.Caret.Location = textArea.TextMargin.PointToLocation(pos);
                textArea.SetCaretToDesiredColumn();
            }
        }
    }

    class CaretDown : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var position = textArea.Caret.Location;
            int lineNr = position.Line;
            int visualLine = textArea.Document.GetVisibleLine(lineNr);
            if (visualLine < textArea.Document.GetVisibleLine(textArea.Document.TotalOfLine))
            {
                Point pos = new Point(textArea.TextMeasurer.GetDrawingXPos(lineNr, position.Column),
                                      textArea.TextMargin.DisplayBounds.Y
                                      + (visualLine + 1) * textArea.TextMeasurer.FontHeight
                                      - textArea.TextMargin.TextArea.VisualLocation.Y);
                textArea.Caret.Location = textArea.TextMargin.PointToLocation(pos);
                textArea.SetCaretToDesiredColumn();
            }
            //			if (textArea.Caret.Line + 1 < textArea.Document.TotalNumberOfLines) {
            //				textArea.SetCaretToDesiredColumn(textArea.Caret.Line + 1);
            //			}
        }
    }

    class WordRight : CaretRight
    {
        public override void Execute(TextArea textArea)
        {
            var line = textArea.Document.GetLineByLineNo(textArea.Caret.Line);
            var pos = textArea.Caret.Location;
            if (textArea.Caret.Column >= line.Length)
            {
                pos = new TextLocation(0, textArea.Caret.Line + 1);
            }
            else
            {
                var nextWordStart = textArea.Document.FindNextWordStart(textArea.Caret.Offset);
                pos = textArea.Document.OffsetToLocation(nextWordStart);
            }

            var marker = textArea.Document.FoldingManager.GetFoldingsFromLocation(pos).FirstOrDefault(f => f.Folded);
            if (marker != null)
            {
                pos = marker.Start;
            }

            textArea.Caret.Location = pos;
            textArea.SetDesiredColumn();
        }
    }

    class WordLeft : CaretLeft
    {
        public override void Execute(TextArea textArea)
        {
            var oldPos = textArea.Caret.Location;
            if (textArea.Caret.Column == 0)
            {
                base.Execute(textArea);
            }
            else
            {
                var line = textArea.Document.GetLineByLineNo(oldPos.Line);
                var prevWordStart = textArea.Document.FindPrevWordStart(textArea.Caret.Offset);
                var newPos = textArea.Document.OffsetToLocation(prevWordStart);
                var marker = textArea.Document.FoldingManager.GetFoldingsFromLocation(newPos).FirstOrDefault(f => f.Folded);
                if (marker != null)
                {
                    oldPos = marker.End;
                }
                textArea.Caret.Location = newPos;
                textArea.SetDesiredColumn();
            }
        }
    }

    class ScrollLineUp : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            textArea.AutoClearSelection = false;
            textArea.TextEditor.VScrollBar.Value = Math.Min(
                textArea.TextEditor.VScrollBar.Maximum,
                textArea.VisualLocation.Y - textArea.TextMeasurer.FontHeight);
        }
    }

    class ScrollLineDown : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            textArea.AutoClearSelection = false;
            textArea.TextEditor.VScrollBar.Value = Math.Min(
                textArea.TextEditor.VScrollBar.Maximum,
                textArea.VisualLocation.Y + textArea.TextMeasurer.FontHeight);
        }
    }
    class Home : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var pos = textArea.Caret.Location;
            while (true)
            {
                var curLine = textArea.Document.GetLineByLineNo(pos.Line);
                if (textArea.Document.IsLineEmpty(curLine))
                {
                    if (pos.Column != 0)
                    {
                        pos.Column = 0;
                    }
                    else
                    {
                        pos.Column = curLine.Length;
                    }
                }
                else
                {
                    int firstCharOffset = textArea.Document.FindFirstNonWSChar(curLine.Offset);
                    int firstCharColumn = firstCharOffset - curLine.Offset;

                    if (pos.Column == firstCharColumn)
                    {
                        pos.Column = 0;
                    }
                    else
                    {
                        pos.Column = firstCharColumn;
                    }
                }
                pos.Column = curLine.Length;
                var marker = textArea.Document.FoldingManager.GetFoldingsFromLocation(pos).FirstOrDefault(f => f.Folded);
                if (marker == null)
                    break;
                pos = marker.End;
            }
            if (pos != textArea.Caret.Location)
            {
                textArea.Caret.Location = pos;
                textArea.SetDesiredColumn();
            }
        }
    }

    class End : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var pos = textArea.Caret.Location;
            while (true)
            {
                var curLine = textArea.Document.GetLineByLineNo(pos.Line);
                pos.Column = curLine.Length;
                var marker = textArea.Document.FoldingManager.GetFoldingsFromLocation(pos).FirstOrDefault(f => f.Folded);
                if (marker == null)
                    break;
                pos = marker.End;
            }

            if (pos != textArea.Caret.Location)
            {
                textArea.Caret.Location = pos;
                textArea.SetDesiredColumn();
            }
        }
    }
    class MoveToStart : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            if (textArea.Caret.Line != 0 || textArea.Caret.Column != 0)
            {
                textArea.Caret.Location = TextLocation.Zero;
                textArea.SetDesiredColumn();
            }
        }
    }
    class MoveToEnd : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            var endPos = textArea.Document.OffsetToLocation(textArea.Document.Length);
            if (textArea.Caret.Location != endPos)
            {
                textArea.Caret.Location = endPos;
                textArea.SetDesiredColumn();
            }
        }
    }
    class Undo : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            textArea.TextEditor.Undo();
        }
    }
    class Redo : KeyActionBase
    {
        public override void Execute(TextArea textArea)
        {
            textArea.TextEditor.Redo();
        }
    }
}
