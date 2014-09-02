using System;
using System.Collections.Generic;

namespace LazyBones.UI.Controls.Editor
{
    public class UndoStack
    {
        Stack<IUndoRedoAction> undostack = new Stack<IUndoRedoAction>();
        Stack<IUndoRedoAction> redostack = new Stack<IUndoRedoAction>();
        TextEditor textEditor;
        public UndoStack(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }

        public TextEditor TextEditor { get { return textEditor; } }

        internal bool AcceptChanges = true;
        public bool CanUndo
        {
            get { return undostack.Count > 0; }
        }
        public bool CanRedo
        {
            get { return redostack.Count > 0; }
        }

        public int UndoActionCount
        {
            get { return undostack.Count; }
        }
        public int RedoActionCount
        {
            get { return redostack.Count; }
        }

        int undoGroupDepth;
        int actionCountInUndoGroup;

        private class UndoGroup : IDisposable
        {
            UndoStack stack;
            public UndoGroup(UndoStack stack)
            {
                this.stack = stack;
                if (stack.undoGroupDepth == 0)
                {
                    stack.actionCountInUndoGroup = 0;
                }
                stack.undoGroupDepth++;
            }
            public void Dispose()
            {
                if (stack.undoGroupDepth == 0)
                    throw new InvalidOperationException("There are no open undo groups");
                stack.undoGroupDepth--;
                if (stack.undoGroupDepth == 0 && stack.actionCountInUndoGroup > 1)
                {
                    stack.undostack.Push(new UndoQueue(stack.undostack, stack.actionCountInUndoGroup));
                }
            }
        }

        public void StartUndoGroup()
        {
            if (undoGroupDepth == 0)
            {
                actionCountInUndoGroup = 0;
            }
            undoGroupDepth++;
        }
        public void EndUndoGroup()
        {
            if (undoGroupDepth == 0)
                throw new InvalidOperationException("There are no open undo groups");
            undoGroupDepth--;
            //Util.LoggingService.Debug("Close undo group (new depth=" + undoGroupDepth + ")");
            if (undoGroupDepth == 0 && actionCountInUndoGroup > 1)
            {
                undostack.Push(new UndoQueue(undostack, actionCountInUndoGroup));
            }
        }
        public void AssertNoUndoGroupOpen()
        {
            if (undoGroupDepth != 0)
            {
                undoGroupDepth = 0;
                throw new InvalidOperationException("No undo group should be open at this point");
            }
        }
        public void Undo()
        {
            AssertNoUndoGroupOpen();
            if (undostack.Count > 0)
            {
                var undo = undostack.Pop();
                redostack.Push(undo);
                undo.Undo();
                OnUndoDone();
            }
        }
        public void Redo()
        {
            AssertNoUndoGroupOpen();
            if (redostack.Count > 0)
            {
                var redo = redostack.Pop();
                undostack.Push(redo);
                redo.Redo();
                OnRedoDone();
            }
        }
        public void Push(IUndoRedoAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (AcceptChanges)
            {
                StartUndoGroup();
                undostack.Push(action);
                actionCountInUndoGroup++;
                if (TextEditor != null)
                {
                    undostack.Push(new UndoableSetCaretPosition(this, TextEditor.Caret.Location));
                    actionCountInUndoGroup++;
                }
                //EndUndoGroup();
                ClearRedoStack();
            }
        }

        public void ClearRedoStack()
        {
            redostack.Clear();
        }
        public void ClearAll()
        {
            AssertNoUndoGroupOpen();
            undostack.Clear();
            redostack.Clear();
            actionCountInUndoGroup = 0;
        }

        public event EventHandler UndoDone;
        protected void OnUndoDone()
        {
            if (UndoDone != null)
            {
                UndoDone(textEditor, null);
            }
        }

        public event EventHandler RedoDone;
        protected void OnRedoDone()
        {
            if (RedoDone != null)
            {
                RedoDone(textEditor, null);
            }
        }
    }
}
