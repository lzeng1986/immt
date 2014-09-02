
namespace LazyBones.UI.Controls.TextEditor
{
    //表示列范围
    public class ColumnRange
    {
        public static readonly ColumnRange Empty = new ColumnRange(-2, -2);
        public static readonly ColumnRange WholeColumn = new ColumnRange(-1, -1);
        public ColumnRange(int startColumn, int endColumn)
        {
            this.StartColumn = startColumn;
            this.EndColumn = endColumn;
        }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public bool IsEmpty
        {
            get { return StartColumn == -2 && EndColumn == -2; }
        }
        public bool IsWholeColumn
        {
            get { return StartColumn == -1 && EndColumn == -1; }
        }

        public override int GetHashCode()
        {
            return StartColumn ^ (EndColumn << 16);
        }

        public override bool Equals(object obj)
        {
            var r = obj as ColumnRange;
            if (ReferenceEquals(r, null))
                return false;
            return r.StartColumn == StartColumn && r.EndColumn == EndColumn;
        }

        public override string ToString()
        {
            return string.Format("[ColumnRange: StartColumn={0}, EndColumn={1}]", StartColumn, EndColumn);
        }
    }
}
