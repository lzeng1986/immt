using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.UI.Controls.TextEditor
{
    class UndoQueue : IUndoRedoAction
    {
        List<IUndoRedoAction> undoActions = new List<IUndoRedoAction>();
        public UndoQueue(Stack<IUndoRedoAction> stack, int actionNum)
        {
            if (stack == null)
                throw new ArgumentNullException("stack");
            if (actionNum < 0)
                throw new ArgumentOutOfRangeException("actionNum");
            undoActions.AddRange(stack.Take(actionNum));
        }
        public void Undo()
        {
            undoActions.ForEach(a => a.Undo());
        }

        public void Redo()
        {
            undoActions.ForEach(a => a.Redo());
        }
    }
}
