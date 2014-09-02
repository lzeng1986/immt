using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 处理剪切板事件
    /// </summary>
    class TextAreaClipboardHandler
    {
        TextArea textArea;
        public TextAreaClipboardHandler(TextArea textArea)
        {
            this.textArea = textArea;
            textArea.SelectionManager.SelectionChanged += SelectionManager_SelectionChanged;
        }

        void SelectionManager_SelectionChanged(object sender, EventArgs e)
        {
        }
        public bool EnableCutOrPaste
        {
            get
            {
                if (textArea.SelectionManager.HasSelection)
                    return !textArea.SelectionManager.ReadOnly;
                else
                    return !textArea.IsReadOnlyAt(textArea.Caret.Offset);
            }
        }
        public bool EnableCut
        {
            get { return EnableCutOrPaste; }
        }

        public bool EnableCopy
        {
            get { return !textArea.SelectionManager.IsEmpty; }
        }

        public bool EnablePaste
        {
            get
            {
                if (!EnableCutOrPaste)
                    return false;
                try
                {
                    return Clipboard.ContainsText();
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool EnableDelete
        {
            get { return !textArea.SelectionManager.IsEmpty && !textArea.SelectionManager.ReadOnly; }
        }

        public bool EnableSelectAll
        {
            get
            {
                return true;
            }
        }
        const string LineSelectedType = "MSDEVLineSelect";
        bool CopyTextToClipboard(string stringToCopy, bool asLine)
        {
            if (string.IsNullOrEmpty(stringToCopy))
                return false;
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.UnicodeText, true, stringToCopy);
            if (asLine)
            {
                MemoryStream lineSelected = new MemoryStream(1);
                lineSelected.WriteByte(1);
                dataObject.SetData(LineSelectedType, false, lineSelected);
            }
            // Default has no highlighting, therefore we don't need RTF output
            //if (textArea.Document.HighlightingStrategy.Name != "Default")
            //{
            //    dataObject.SetData(DataFormats.Rtf, RtfWriter.GenerateRtf(textArea));
            //}
            OnCopyText(new CopyTextEventArgs(stringToCopy));

            SafeSetClipboard(dataObject);
            return true;
        }

        // Code duplication: TextAreaClipboardHandler.cs also has SafeSetClipboard
        [ThreadStatic]
        static int SafeSetClipboardDataVersion;

        static void SafeSetClipboard(object dataObject)
        {
            // Work around ExternalException bug. (SD2-426)
            // Best reproducable inside Virtual PC.
            int version = unchecked(++SafeSetClipboardDataVersion);
            try
            {
                Clipboard.SetDataObject(dataObject, true);
            }
            catch (ExternalException)
            {
                Timer timer = new Timer();
                timer.Interval = 100;
                timer.Tick += delegate
                {
                    timer.Stop();
                    timer.Dispose();
                    if (SafeSetClipboardDataVersion == version)
                    {
                        try
                        {
                            Clipboard.SetDataObject(dataObject, true, 10, 50);
                        }
                        catch (ExternalException) { }
                    }
                };
                timer.Start();
            }
        }

        bool CopyTextToClipboard(string stringToCopy)
        {
            return CopyTextToClipboard(stringToCopy, false);
        }

        public void Cut()
        {
            if (!textArea.SelectionManager.IsEmpty)
            {
                if (CopyTextToClipboard(textArea.SelectionManager.SelectedText))
                {
                    if (textArea.SelectionManager.ReadOnly)
                        return;
                    // Remove text
                    textArea.BeginUpdate();
                    textArea.Caret.Location = textArea.SelectionManager.Selections[0].Start;
                    textArea.SelectionManager.RemoveSelectedText();
                    textArea.EndUpdate();
                }
            }
            else if (textArea.Document.EditorOptions.CutCopyWholeLine)
            {
                // No text was selected, select and cut the entire line
                var curLineNr = textArea.Document.OffsetToLineNo(textArea.Caret.Offset);
                var lineWhereCaretIs = textArea.Document.GetLineByLineNo(curLineNr);
                var caretLineText = textArea.Document.GetText(lineWhereCaretIs.Offset, lineWhereCaretIs.TotalLength);
                textArea.SelectionManager.SetSelection(textArea.Document.OffsetToLocation(lineWhereCaretIs.Offset), textArea.Document.OffsetToLocation(lineWhereCaretIs.Offset + lineWhereCaretIs.TotalLength));
                if (CopyTextToClipboard(caretLineText, true))
                {
                    if (textArea.SelectionManager.ReadOnly)
                        return;
                    // remove line
                    textArea.BeginUpdate();
                    textArea.Caret.Location = textArea.Document.OffsetToLocation(lineWhereCaretIs.Offset);
                    textArea.SelectionManager.RemoveSelectedText();
                    textArea.Document.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.PositionToEnd, new TextLocation(0, curLineNr)));
                    textArea.EndUpdate();
                }
            }
        }

        public void Copy()
        {
            if (!CopyTextToClipboard(textArea.SelectionManager.SelectedText) && textArea.Document.EditorOptions.CutCopyWholeLine)
            {
                // No text was selected, select the entire line, copy it, and then deselect
                int curLineNr = textArea.Document.OffsetToLineNo(textArea.Caret.Offset);
                var lineWhereCaretIs = textArea.Document.GetLineByLineNo(curLineNr);
                string caretLineText = textArea.Document.GetText(lineWhereCaretIs.Offset, lineWhereCaretIs.TotalLength);
                CopyTextToClipboard(caretLineText, true);
            }
        }

        public void Paste()
        {
            if (!EnableCutOrPaste)
                return;
            // Clipboard.GetDataObject may throw an exception...
            for (int i = 0; ; i++)
            {
                try
                {
                    IDataObject data = Clipboard.GetDataObject();
                    if (data == null)
                        return;
                    bool fullLine = data.GetDataPresent(LineSelectedType);
                    if (data.GetDataPresent(DataFormats.UnicodeText))
                    {
                        string text = (string)data.GetData(DataFormats.UnicodeText);
                        if (text.Length > 0)
                        {
                            textArea.Document.UndoStack.StartUndoGroup();
                            try
                            {
                                if (!textArea.SelectionManager.IsEmpty)
                                {
                                    textArea.Caret.Location = textArea.SelectionManager.Selections[0].Start;
                                    textArea.SelectionManager.RemoveSelectedText();
                                }
                                if (fullLine)
                                {
                                    int col = textArea.Caret.Column;
                                    textArea.Caret.Column = 0;
                                    if (!textArea.IsReadOnlyAt(textArea.Caret.Offset))
                                        textArea.Insert(text);
                                    textArea.Caret.Column = col;
                                }
                                else
                                {
                                    // textArea.EnableCutOrPaste already checked readonly for this case
                                    textArea.Insert(text);
                                }
                            }
                            finally
                            {
                                textArea.Document.UndoStack.EndUndoGroup();
                            }
                        }
                    }
                    return;
                }
                catch (ExternalException)
                {
                    // GetDataObject does not provide RetryTimes parameter
                    if (i > 5) throw;
                }
            }
        }

        public void Delete(object sender, EventArgs e)
        {
            //new Delete().Execute(textArea);
        }

        public void SelectAll(object sender, EventArgs e)
        {
            //new ICSharpCode.TextEditor.Actions.SelectWholeDocument().Execute(textArea);
        }

        protected virtual void OnCopyText(CopyTextEventArgs e)
        {
            if (TextCopied != null)
            {
                TextCopied(this, e);
            }
        }

        public event EventHandler<CopyTextEventArgs> TextCopied;
    }
    public class CopyTextEventArgs : EventArgs
    {
        string text;

        public string Text
        {
            get
            {
                return text;
            }
        }

        public CopyTextEventArgs(string text)
        {
            this.text = text;
        }
    }
}
