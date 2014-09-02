using System;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 表示一个锚点，表示编辑器中一个固定的位置，可以随文本移动
    /// </summary>
    public class TextAnchor
    {
        TextLine line;
        int columnNo;
        internal TextAnchor(TextLine line, int columnNo)
        {
            this.line = line;
            this.columnNo = columnNo;
        }

        public AnchorType Type { get; set; }
        public TextLine Line
        {
            get
            {
                if (line == null)
                    throw new InvalidOperationException();
                return line;
            }
            set { line = value; }
        }
        public int LineNo
        {
            get { return Line.LineNo; }
        }
        public int ColumnNo
        {
            get
            {
                if (line == null)
                    throw new InvalidOperationException();
                return columnNo;
            }
            internal set { columnNo = value; }
        }
        public TextLocation Location
        {
            get { return new TextLocation(this.ColumnNo, this.LineNo); }
        }
        public bool IsDeleted
        {
            get { return line == null; }
        }
        internal void Delete(LazyDeleter deleter)
        {
            line = null;
            deleter.AddRemovedAnchor(this);
        }
        public event EventHandler Deleted;
        internal void RaiseDeleted()
        {
            if (Deleted != null)
                Deleted(this, EventArgs.Empty);
        }
    }
    public enum AnchorType
    {
        BeforeInsert,
        AfterInsert
    }
}
