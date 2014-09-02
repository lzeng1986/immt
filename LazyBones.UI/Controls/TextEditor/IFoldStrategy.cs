using System.Collections.Generic;

namespace LazyBones.UI.Controls.TextEditor
{
    public interface IFoldStrategy
    {
        List<FoldMarker> GenerateFoldMarkers(Document document, string fileName, object parseInformation);
    }
    class DefaultFoldStrategy : IFoldStrategy
    {
        public List<FoldMarker> GenerateFoldMarkers(Document document, string fileName, object parseInformation)
        {
            List<FoldMarker> l = new List<FoldMarker>();
            Stack<int> offsetStack = new Stack<int>();
            Stack<string> textStack = new Stack<string>();
            //int level = 0;
            //foreach (LineSegment segment in document.LineSegmentCollection) {
            //	
            //}
            return l;
        }

        int GetLevel(Document document, int offset)
        {
            int level = 0;
            int spaces = 0;
            for (int i = offset; i < document.Length; ++i)
            {
                char c = document.GetChar(i);
                if (c == '\t' || (c == ' ' && ++spaces == 4))
                {
                    spaces = 0;
                    ++level;
                }
                else
                {
                    break;
                }
            }
            return level;
        }
    }
}
