using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace LazyBones.UI.Controls.TextEditor
{
    //处理TextArea键盘操作
    class TextAreaKeyHandler
    {
        Dictionary<Keys, IKeyAction> actions = new Dictionary<Keys, IKeyAction>();
        public TextAreaKeyHandler()
        {
            AddDefaultActions();
        }
        public IKeyAction this[Keys keyData]
        {
            get
            {
                IKeyAction action;
                actions.TryGetValue(keyData, out action);
                return action;
            }
            set { actions[keyData] = value; }
        }

        public void Load(XmlElement xml)
        {

        }
        void AddDefaultActions()
        {
            actions[Keys.Left] = new CaretLeft();
            //editactions[Keys.Left | Keys.Shift] = new ShiftCaretLeft();
            //editactions[Keys.Left | Keys.Control] = new WordLeft();
            //editactions[Keys.Left | Keys.Control | Keys.Shift] = new ShiftWordLeft();
            //editactions[Keys.Right] = new Right();
            //editactions[Keys.Right | Keys.Shift] = new ShiftCaretRight();
            //editactions[Keys.Right | Keys.Control] = new WordRight();
            //editactions[Keys.Right | Keys.Control | Keys.Shift] = new ShiftWordRight();
            //editactions[Keys.Up] = new CaretUp();
            //editactions[Keys.Up | Keys.Shift] = new ShiftCaretUp();
            //editactions[Keys.Up | Keys.Control] = new ScrollLineUp();
            //editactions[Keys.Down] = new CaretDown();
            //editactions[Keys.Down | Keys.Shift] = new ShiftCaretDown();
            //editactions[Keys.Down | Keys.Control] = new ScrollLineDown();

            //editactions[Keys.Insert] = new ToggleEditMode();
            //editactions[Keys.Insert | Keys.Control] = new Copy();
            //editactions[Keys.Insert | Keys.Shift] = new Paste();
            //editactions[Keys.Delete] = new Delete();
            //editactions[Keys.Delete | Keys.Shift] = new Cut();
            //editactions[Keys.Home] = new Home();
            //editactions[Keys.Home | Keys.Shift] = new ShiftHome();
            //editactions[Keys.Home | Keys.Control] = new MoveToStart();
            //editactions[Keys.Home | Keys.Control | Keys.Shift] = new ShiftMoveToStart();
            //editactions[Keys.End] = new End();
            //editactions[Keys.End | Keys.Shift] = new ShiftEnd();
            //editactions[Keys.End | Keys.Control] = new MoveToEnd();
            //editactions[Keys.End | Keys.Control | Keys.Shift] = new ShiftMoveToEnd();
            //editactions[Keys.PageUp] = new MovePageUp();
            //editactions[Keys.PageUp | Keys.Shift] = new ShiftMovePageUp();
            //editactions[Keys.PageDown] = new MovePageDown();
            //editactions[Keys.PageDown | Keys.Shift] = new ShiftMovePageDown();

            //editactions[Keys.Return] = new Return();
            //editactions[Keys.Tab] = new Tab();
            //editactions[Keys.Tab | Keys.Shift] = new ShiftTab();
            //editactions[Keys.Back] = new Backspace();
            //editactions[Keys.Back | Keys.Shift] = new Backspace();

            actions[Keys.X | Keys.Control] = new Cut();
            actions[Keys.C | Keys.Control] = new Copy();
            actions[Keys.V | Keys.Control] = new Paste();

            //editactions[Keys.A | Keys.Control] = new SelectWholeDocument();
            //editactions[Keys.Escape] = new ClearAllSelections();

            //editactions[Keys.Divide | Keys.Control] = new ToggleComment();
            //editactions[Keys.OemQuestion | Keys.Control] = new ToggleComment();

            //editactions[Keys.Back | Keys.Alt] = new Undo();
            //editactions[Keys.Z | Keys.Control] = new Undo();
            //editactions[Keys.Y | Keys.Control] = new Redo();

            //editactions[Keys.Delete | Keys.Control] = new DeleteWord();
            //editactions[Keys.Back | Keys.Control] = new WordBackspace();
            //editactions[Keys.D | Keys.Control] = new DeleteLine();
            //editactions[Keys.D | Keys.Shift | Keys.Control] = new DeleteToLineEnd();

            //editactions[Keys.B | Keys.Control] = new GotoMatchingBrace();
        }
    }
}
