using System.Drawing;

namespace LazyBones.UI.Controls.TextEditor
{
    class TextWord
    {
        public static readonly TextWord Tab;
        public static readonly TextWord Space;
        static TextWord()
        {
            Tab = new TextWord();
            Tab.Type = WordType.Tab;
            Tab.Length = 1;

            Space = new TextWord();
            Space.Type = WordType.Space;
            Space.Length = 1;

        }
        Document doc;

        private TextWord() { }
        public TextWord(Document doc, TextLine line, int offset, int length, TextDrawInfo color)
        {
            this.doc = doc;
            this.Line = line;
            Offset = offset;
            Length = length;
            SyntaxColor = color;
            Type = WordType.Word;
        }

        public TextLine Line { get; private set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }
        public WordType Type { get; private set; }
        public TextDrawInfo SyntaxColor { get; internal set; }
        public Color Color
        {
            get { return SyntaxColor == null ? Color.Black : SyntaxColor.BackColor; }
        }
        public string Text
        {
            get { return doc == null ? string.Empty : doc.GetText(Offset, Length); }
        }
    }

    public enum WordType
    {
        Space,
        Tab,
        Word
    }
}
