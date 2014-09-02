using System;

namespace LazyBones.UI.Controls.Editor
{
    public class BracketHighlight
    {
        public TextLocation OpenBrace { get; set; }
        public TextLocation CloseBrace { get; set; }

        public BracketHighlight(TextLocation openBrace, TextLocation closeBrace)
        {
            this.OpenBrace = openBrace;
            this.CloseBrace = closeBrace;
        }
    }
    public class BracketHighlightingSheme
    {
        public char OpenTag{get;set;}

        public char ClosingTag{get;set;}

        public BracketHighlightingSheme(char opentag, char closingtag)
        {
            OpenTag = opentag;
            ClosingTag = closingtag;
        }

        public BracketHighlight GetHighlight(Document document, int offset)
        {
            int searchOffset;
            if (document.EditorOptions.BracketHighlightStyle == BracketHighlightStyle.AfterBracket)
            {
                searchOffset = offset;
            }
            else
            {
                searchOffset = offset + 1;
            }
            char word = document.GetChar(Math.Max(0, Math.Min(document.Length - 1, searchOffset)));

            //Location endP = document.OffsetToLocation(searchOffset);
            //if (word == OpenTag)
            //{
            //    if (searchOffset < document.Length)
            //    {
            //        int bracketOffset = TextUtilities.SearchBracketForward(document, searchOffset + 1, opentag, closingtag);
            //        if (bracketOffset >= 0)
            //        {
            //            var p = document.OffsetToLocation(bracketOffset);
            //            return new BracketHighlight(p, endP);
            //        }
            //    }
            //}
            //else if (word == ClosingTag)
            //{
            //    if (searchOffset > 0)
            //    {
            //        int bracketOffset = TextUtilities.SearchBracketBackward(document, searchOffset - 1, opentag, closingtag);
            //        if (bracketOffset >= 0)
            //        {
            //            var p = document.OffsetToLocation(bracketOffset);
            //            return new BracketHighlight(p, endP);
            //        }
            //    }
            //}
            return null;
        }
    }
}
