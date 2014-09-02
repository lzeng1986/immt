using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.Text;

namespace LazyBones.UI.Controls.TextEditor
{
    //编辑器的设置选项
    [Serializable]
    public class EditorOptions
    {
        public EditorOptions()
        {
            ResetSetting();
        }
        int tabIndent = 4;
        [DefaultValue(4)]
        public int TabIndent
        {
            get { return tabIndent; }
            set { tabIndent = value; }
        }
        bool lineNoVisible = true;
        [DefaultValue(true)]
        public bool LineNoVisible
        {
            get { return lineNoVisible; }
            set { lineNoVisible = value; }
        }
        bool whiteSpaceVisible = false;
        [DefaultValue(false)]
        public bool WhiteSpaceVisible
        {
            get { return whiteSpaceVisible; }
            set { whiteSpaceVisible = value; }
        }
        bool tabVisible = false;
        [DefaultValue(false)]
        public bool TabVisible
        {
            get { return tabVisible; }
            set { tabVisible = value; }
        }
        bool eolMarkerVisible = false;
        [DefaultValue(false)]
        public bool EOLMarkerVisible
        {
            get { return eolMarkerVisible; }
            set { eolMarkerVisible = value; }
        }
        bool enableFolding = true;
        [DefaultValue(true)]
        public bool EnableFolding
        {
            get { return enableFolding; }
            set { enableFolding = value; }
        }
        public TextRenderingHint TextRenderingHint { get; set; }
        bool cursorVisible = true;
        [DefaultValue(true)]
        public bool CursorVisible
        {
            get { return cursorVisible; }
            set { cursorVisible = value; }
        }
        Encoding encoding = Encoding.UTF8;
        [DefaultValue(typeof(Encoding), "UTF8")]
        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }
        SelectionMode selectionMode = SelectionMode.Normal;
        [DefaultValue(SelectionMode.Normal)]
        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set { selectionMode = value; }
        }
        string lineTerminator = Environment.NewLine;
        [DefaultValue(typeof(Environment), "NewLine")]
        public string LineTerminator
        {
            get { return lineTerminator; }
            set { lineTerminator = value; }
        }
        bool autoInsertCurlyBracket = false;
        public bool AutoInsertCurlyBracket
        {
            get { return autoInsertCurlyBracket; }
            set { autoInsertCurlyBracket = value; }
        }
        TextFontContainer font = TextFontContainer.Default;
        [DefaultValue(typeof(TextFontContainer), "Default")]
        public TextFontContainer Font
        {
            get { return font; }
            set { font = value; }
        }
        IndentStyle indentStyle = IndentStyle.None;
        [DefaultValue(IndentStyle.None)]
        public IndentStyle IndentStyle
        {
            get { return indentStyle; }
            set { indentStyle = value; }
        }
        BracketHighlightStyle bracketHighlightStyle = BracketHighlightStyle.None;
        [DefaultValue(BracketHighlightStyle.None)]
        public BracketHighlightStyle BracketHighlightStyle
        {
            get { return bracketHighlightStyle; }
            set { bracketHighlightStyle = value; }
        }
        bool highlightCurrentLine = true;
        [DefaultValue(true)]
        public bool HighlightCurrentLine
        {
            get { return highlightCurrentLine; }
            set { highlightCurrentLine = value; }
        }
        bool supportReadOnlySegment = false;
        [DefaultValue(false)]
        public bool SupportReadOnlySegment
        {
            get { return supportReadOnlySegment; }
            set { supportReadOnlySegment = value; }
        }
        bool invalidLinesVisible = false;
        [DefaultValue(false)]
        public bool InvalidLinesVisible
        {
            get { return invalidLinesVisible; }
            set { invalidLinesVisible = value; }
        }
        bool verticalRulerVisible = true;
        [DefaultValue(true)]
        public bool VerticalRulerVisible
        {
            get { return verticalRulerVisible; }
            set { verticalRulerVisible = value; }
        }
        int verticalRulerCol = 80;
        [DefaultValue(80)]
        public int VerticalRulerCol
        {
            get { return verticalRulerCol; }
            set { verticalRulerCol = value; }
        }
        bool showMatchingBracket = true;
        [DefaultValue(true)]
        public bool ShowMatchingBracket
        {
            get { return showMatchingBracket; }
            set { showMatchingBracket = value; }
        }
        [DefaultValue(false)]
        public bool CutCopyWholeLine { get; set; }
        public void ResetSetting()
        {
            TabIndent = 4;
            LineNoVisible = true;
            WhiteSpaceVisible = false;
            TabVisible = false;
            EOLMarkerVisible = false;
            EnableFolding = true;
            CursorVisible = true;
            Encoding = Encoding.UTF8;
            Font = TextFontContainer.Default;
        }
    }
    public enum IndentStyle
    {
        None,
        Auto,
        Smart
    }

    public enum BracketHighlightStyle
    {
        None,
        OnBracket,
        AfterBracket
    }

    public enum SelectionMode
    {
        Normal,
        MultiSelect
    }
}
