using System.Linq;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 表示文档中选择部分
    /// </summary>
    public class Selection
    {
        Document doc;
        public Selection(Document doc)
            : this(doc, TextLocation.Zero, TextLocation.Zero)
        {
        }
        public Selection(Document doc, TextLocation start, TextLocation end)
        {
            this.doc = doc;
            Start = start;
            End = end;
        }
        TextLocation start;
        public TextLocation Start
        {
            get { return start; }
            set
            {
                start = value;
                startOffset = doc.LocationToOffset(value);
            }
        }
        TextLocation end;
        public TextLocation End
        {
            get { return end; }
            set
            {
                end = value;
                endOffset = doc.LocationToOffset(value);
            }
        }
        int startOffset = 0;
        public int StartOffset
        {
            get { return startOffset; }
        }
        int endOffset = 0;
        public int EndOffset
        {
            get { return endOffset; }
        }

        public int Length
        {
            get { return endOffset - startOffset; }
        }

        public string Text
        {
            get { return Length < 0 ? string.Empty : doc.GetText(startOffset, endOffset - startOffset); }
        }

        public bool IsEmpty
        {
            get { return startOffset == endOffset; }
        }
        public bool Contain(int offset)
        {
            return startOffset <= offset && offset <= endOffset;
        }
        public bool Contain(TextLocation location)
        {
            return start <= location && location <= end;
        }
        public bool IsOverlap(Selection other)
        {
            return (startOffset <= other.startOffset && other.startOffset <= endOffset) ||
                (startOffset <= other.endOffset && other.endOffset <= endOffset) ||
                (other.startOffset <= startOffset && endOffset <= other.endOffset);
        }
        public bool ReadOnly
        {
            get
            {
                if (doc.EditorOptions.SupportReadOnlySegment)
                    return doc.TextMarkerManager.GetMarkers(startOffset, Length).Any(m => m.ReadOnly);
                return false;
            }
        }
        public override bool Equals(object obj)
        {
            var b = obj as Selection;
            if (ReferenceEquals(b, null))
                return false;
            return (this == b);
        }
        public override int GetHashCode()
        {
            return startOffset ^ endOffset;
        }
        public static bool operator ==(Selection a, Selection b)
        {
            return a.doc == b.doc && a.start == b.start && a.end == b.end;
        }
        public static bool operator !=(Selection a, Selection b)
        {
            return !(a == b);
        }
    }
}
