using System;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 表示文档中的位置
    /// </summary>
    public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>
    {
        public static TextLocation Zero = new TextLocation(0, 0);
        int column;
        int line;
        public TextLocation(int column, int line)
        {
            this.column = column;
            this.line = line;
        }
        public int Column
        {
            get { return column; }
            set { column = value; }
        }
        public int Line
        {
            get { return line; }
            set { Line = value; }
        }
        public bool IsZero
        {
            get { return line <= 0 && column <= 0; }
        }
        public override bool Equals(object obj)
        {
            if (obj is TextLocation)
                return Equals((TextLocation)obj);
            return false;
        }
        public override int GetHashCode()
        {
            return unchecked(87 * column.GetHashCode() ^ line.GetHashCode());
        }
        public bool Equals(TextLocation other)
        {
            return column == other.column && line == other.line;
        }
        public int CompareTo(TextLocation other)
        {
            if (line == other.line)
                return column.CompareTo(other.column);
            return line.CompareTo(other.line);
        }
        public static bool operator ==(TextLocation a, TextLocation b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(TextLocation a, TextLocation b)
        {
            return !a.Equals(b);
        }
        public static bool operator <=(TextLocation a, TextLocation b)
        {
            if (a.line == b.line)
                return a.column <= b.column;
            return a.line <= b.line;
        }
        public static bool operator <(TextLocation a, TextLocation b)
        {
            if (a.line == b.line)
                return a.column < b.column;
            return a.line < b.line;
        }
        public static bool operator >=(TextLocation a, TextLocation b)
        {
            if (a.line == b.line)
                return a.column >= b.column;
            return a.line >= b.line;
        }
        public static bool operator >(TextLocation a, TextLocation b)
        {
            if (a.line == b.line)
                return a.column > b.column;
            return a.line > b.line;
        }
    }
}
