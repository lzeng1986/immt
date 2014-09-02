using System;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 定义undo、redo接口
    /// </summary>
    public interface IUndoRedoAction
    {
        void Undo();
        void Redo();
    }
    public class UndoDelete : IUndoRedoAction
    {
        Document document;
        int offset;
        string text;

        public UndoDelete(Document document, int offset, string text)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (offset < 0 || document.Length <= offset)
                throw new ArgumentOutOfRangeException("offset");
            if (text == null)
                throw new ArgumentNullException("text");
            this.document = document;
            this.offset = offset;
            this.text = text;
        }
        public void Undo()
        {
            document.Insert(offset, text);
        }

        public void Redo()
        {
            document.Remove(offset, text.Length);
        }
    }
    public class UndoInsert : IUndoRedoAction
    {
        Document document;
        int offset;
        string text;
        public UndoInsert(Document document, int offset, string text)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (offset < 0 || document.Length < offset)
                throw new ArgumentOutOfRangeException("offset");
            this.document = document;
            this.offset = offset;
            this.text = text;
        }
        public void Undo()
        {
            document.UndoStack.AcceptChanges = false;
            document.Remove(offset, text.Length);
            document.UndoStack.AcceptChanges = true;
        }
        public void Redo()
        {
            document.UndoStack.AcceptChanges = false;
            document.Insert(offset, text);
            document.UndoStack.AcceptChanges = true;
        }
    }
    public class UndoableReplace : IUndoRedoAction
    {
        Document document;
        int offset;
        string text;
        string origText;

        public UndoableReplace(Document document, int offset, string origText, string text)
        {
            if (document == null)
                throw new ArgumentNullException("document");
            if (offset < 0 || document.Length <= offset)
                throw new ArgumentOutOfRangeException("offset");
            if (origText == null)
                throw new ArgumentNullException("origText");
            if (text == null)
                throw new ArgumentNullException("text");
            this.document = document;
            this.offset = offset;
            this.text = text;
            this.origText = origText;
        }
        public void Undo()
        {
            document.UndoStack.AcceptChanges = false;
            document.Replace(offset, text.Length, origText);
            document.UndoStack.AcceptChanges = true;
        }
        public void Redo()
        {
            document.UndoStack.AcceptChanges = false;
            document.Replace(offset, origText.Length, text);
            document.UndoStack.AcceptChanges = true;
        }
    }
    class UndoableSetCaretPosition : IUndoRedoAction
    {
        UndoStack stack;
        TextLocation undoPos;
        TextLocation redoPos;

        public UndoableSetCaretPosition(UndoStack stack, TextLocation pos)
        {
            this.stack = stack;
            this.undoPos = pos;
        }

        public void Undo()
        {
            redoPos = stack.TextEditor.Caret.Location;
            stack.TextEditor.Caret.Location = undoPos;
        }

        public void Redo()
        {
            stack.TextEditor.Caret.Location = redoPos;
        }
    }
}
