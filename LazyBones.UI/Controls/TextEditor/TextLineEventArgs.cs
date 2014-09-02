using System;

namespace LazyBones.UI.Controls.TextEditor
{
    public class TextLineEventArgs : EventArgs
    {
        public Document Document { get; private set; }

        public TextLine Line { get; private set; }

        public TextLineEventArgs(Document document, TextLine line)
        {
            Document = document;
            Line = line;
        }

        public override string ToString()
        {
            return string.Format("[LineEventArgs Document={0} LineSegment={1}]", Document, Line);
        }
    }
    public class LineLengthChangedEventArgs : TextLineEventArgs
    {
        public int Offset{get;private set;}

        public LineLengthChangedEventArgs(Document document, TextLine line, int offset)
            : base(document, line)
        {
            Offset = offset;
        }

        public override string ToString()
        {
            return string.Format("[Document={0} LineSegment={1} Offset={2}]", Document, Line, Offset);
        }
    }
    public class LineCountChangedEventArgs : EventArgs
    {
        public Document Document { get; private set; }
        public int LineStart { get; private set; }
        public int Offset { get; private set; }

        public LineCountChangedEventArgs(Document document, int lineStart, int offset)
        {
            Document = document;
            LineStart = lineStart;
            Offset = offset;
        }
    }
}
