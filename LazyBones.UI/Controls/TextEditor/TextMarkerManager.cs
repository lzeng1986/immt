using System;
using System.Collections.Generic;
using System.Linq;

namespace LazyBones.UI.Controls.TextEditor
{
    public class TextMarkerManager
    {
        List<TextMarker> textMarkers = new List<TextMarker>();
        Document document;
        public TextMarkerManager(Document document)
        {
            this.document = document;
            document.DocumentChanged += DocumentChanged;
        }
        void DocumentChanged(object sender, DocumentEventArgs e)
        {
            document.UpdateSegmentListOnDocumentChange(textMarkers, e);
        }
        public Document Document { get { return document; } }
        public IList<TextMarker> TextMarkers
        {
            get { return textMarkers; }
        }
        public void AddMarker(TextMarker item)
        {
            textMarkers.Add(item);
        }
        public void InsertMarker(int index, TextMarker item)
        {
            textMarkers.Insert(index, item);
        }
        public bool RemoveMarker(TextMarker item)
        {
            return textMarkers.Remove(item);
        }
        public void RemoveAll(Predicate<TextMarker> match)
        {
            textMarkers.RemoveAll(match);
        }
        public IEnumerable<TextMarker> GetMarkers(int offset)
        {
            return textMarkers.Where(marker => marker.Offset <= offset && offset <= marker.EndOffset);
        }
        public IEnumerable<TextMarker> GetMarkers(int offset, int length)
        {
            int endOffset = offset + length - 1;
            return textMarkers.Where(marker =>marker.Offset <= offset && offset <= marker.EndOffset ||
                    // end in marker region
                    marker.Offset <= endOffset && endOffset <= marker.EndOffset ||
                    // marker start in region
                    offset <= marker.Offset && marker.Offset <= endOffset ||
                    // marker end in region
                    offset <= marker.EndOffset && marker.EndOffset <= endOffset
                );
        }
        public IEnumerable<TextMarker> GetMarkers(TextLocation position)
        {
            if (position.Line < 0 || document.TotalOfLine <= position.Line)
                return Enumerable.Empty<TextMarker>();
            var line = document.GetLineByLineNo(position.Line);
            return GetMarkers(line.Offset + position.Column);
        }
    }
}
