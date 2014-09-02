using System.Collections.Generic;

namespace LazyBones.UI.Controls.Editor
{
    public interface IHighlighter
    {
        string Name { get; }
        string[] Extensions { get; set; }
        Dictionary<string, string> Properties { get; }
        TextDrawInfo this[string name] { get; set; }
        void MarkTokens(Document document, List<TextLine> lines);
        void MarkTokens(Document document);
    }
    public class Highlighter : IHighlighter
    {
        Dictionary<string, TextDrawInfo> colors = new Dictionary<string, TextDrawInfo>();
        TextDrawInfo defaultColor;
        public Highlighter(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }

        public string[] Extensions { get; set; }

        public Dictionary<string, string> Properties
        {
            get { throw new System.NotImplementedException(); }
        }
        public TextDrawInfo DefaultColor { get { return defaultColor; } }
        public TextDrawInfo this[string name]
        {
            get
            {
                if (colors.ContainsKey(name))
                    return colors[name];
                return defaultColor;
            }
            set
            {
                if (name.Equals("default", System.StringComparison.InvariantCultureIgnoreCase))
                    defaultColor = value;
                colors[name] = value;
            }
        }
        //public HLColor GetColor(Document document, Line currentSegment, int currentOffset, int currentLength)
        //{
        //    return GetColor(defaultRuleSet, document, currentSegment, currentOffset, currentLength);
        //}

        //protected virtual HLColor GetColor(HighlightRuleSet ruleSet, Document document, Line currentSegment, int currentOffset, int currentLength)
        //{
        //    if (ruleSet != null)
        //    {
        //        if (ruleSet.Reference != null)
        //        {
        //            return ruleSet.Highlighter.GetColor(document, currentSegment, currentOffset, currentLength);
        //        }
        //        else
        //        {
        //            return (HLColor)ruleSet.KeyWords[document, currentSegment, currentOffset, currentLength];
        //        }
        //    }
        //    return null;
        //}
        public void MarkTokens(Document document, List<TextLine> lines)
        {
            throw new System.NotImplementedException();
        }

        public void MarkTokens(Document document)
        {
            throw new System.NotImplementedException();
        }
    }
}
