using System;
using System.Collections.Generic;

namespace LazyBones.UI.Controls.TextEditor
{
    public class FoldMarker : MarkerBase, IComparable<FoldMarker>
    {
        Document doc;
        public FoldMarker(Document doc, int offset, int length, string foldText, bool folded)
        {
            this.doc = doc;
            this.offset = offset;
            this.length = length;
            this.FoldText = foldText;
            this.Folded = folded;
        }
        public FoldMarker(Document doc, int startLine, int startCol, int endLine, int endCol)
            : this(doc, new TextLocation(startLine, startCol), new TextLocation(endLine, endCol))
        {
        }
        public FoldMarker(Document doc, TextLocation start, TextLocation end)
            : this(doc, start, end, FoldMarkerType.Unspecified)
        {
        }

        public FoldMarker(Document doc, TextLocation start, TextLocation end, FoldMarkerType foldType)
            : this(doc, start, end, foldType, "...")
        {
        }
        public FoldMarker(Document doc, TextLocation start, TextLocation end, FoldMarkerType foldType, string foldText)
            : this(doc, start, end, foldType, foldText, false)
        {
        }
        public FoldMarker(Document doc, TextLocation start, TextLocation end, FoldMarkerType foldType, string foldText, bool folded)
        {
            this.doc = doc;
            this.start = start;
            this.end = end;

            if (string.IsNullOrEmpty(foldText))
                foldText = "...";

            this.MarkerType = foldType;
            this.FoldText = foldText;

            var startLine = doc.GetLineByLineNo(start.Line);
            var endLine = doc.GetLineByLineNo(end.Line);
            this.offset = startLine.Offset + Math.Min(start.Column, startLine.Length);
            this.length = (endLine.Offset + Math.Min(end.Column, endLine.Length)) - this.offset;

            this.Folded = folded;
        }

        public FoldMarkerType MarkerType { get; set; }

        TextLocation start = TextLocation.Zero;
        public TextLocation Start
        {
            get { return start; }
        }
        TextLocation end = TextLocation.Zero;
        public TextLocation End
        {
            get { return end; }
        }
        public override int Offset
        {
            get { return base.Offset; }
            set
            {
                base.Offset = value;
                start = doc.OffsetToLocation(value);
                end = doc.OffsetToLocation(offset + length);
            }
        }
        public override int Length
        {
            get { return base.Length; }
            set
            {
                base.Length = value;
                end = doc.OffsetToLocation(offset + length);
            }
        }

        public bool Folded { get; set; }

        public string FoldText { get; private set; }

        public string HiddenText
        {
            get { return doc.GetText(offset, length); }
        }
        public int CompareTo(FoldMarker other)
        {
            if (offset == other.offset)
                return length.CompareTo(other.length);
            return offset.CompareTo(other.offset);
        }
    }
    static class FoldMarkerComparer
    {
        public static readonly IComparer<FoldMarker> ByStart = new StartComparer();
        public static readonly IComparer<FoldMarker> ByEnd = new EndComparer();
        private class StartComparer : IComparer<FoldMarker>
        {
            public int Compare(FoldMarker x, FoldMarker y)
            {
                var c = x.Start.Line.CompareTo(x.Start.Line);
                if (c == 0)
                    return x.Start.Column.CompareTo(x.Start.Column);
                return c;
            }
        }
        private class EndComparer : IComparer<FoldMarker>
        {
            public int Compare(FoldMarker x, FoldMarker y)
            {
                var c = x.End.Line.CompareTo(x.End.Line);
                if (c == 0)
                    return x.End.Column.CompareTo(x.End.Column);
                return c;
            }
        }
    }
    public enum FoldMarkerType
    {
        Unspecified,
        MemberBody,
        Region,
        TypeBody
    }
}
