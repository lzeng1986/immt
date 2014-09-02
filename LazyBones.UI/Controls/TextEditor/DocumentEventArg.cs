using System;

namespace LazyBones.UI.Controls.TextEditor
{
    public class DocumentEventArgs : EventArgs
    {
        public Document Document { get; private set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }
        public string Text { get; private set; }
        public DocumentEventArgs(Document document)
            : this(document, -1, -1, null)
        {
        }
        public DocumentEventArgs(Document document, int offset)
            : this(document, offset, -1, null)
        {
        }
        public DocumentEventArgs(Document document, int offset, int length)
            : this(document, offset, length, null)
        {
        }
        public DocumentEventArgs(Document document, int offset, int length, string text)
        {
            Document = document;
            Offset = offset;
            Length = length;
            Text = text;
        }
        public override string ToString()
        {
            return String.Format("[DocumentEventArgs: Document = {0}, Offset = {1}, Text = {2}, Length = {3}]", Document, Offset, Text, Length);
        }
    }
}
