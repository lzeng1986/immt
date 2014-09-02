using System.Collections.Generic;

namespace LazyBones.UI.Controls.Editor
{
    class LazyDeleter
    {
        internal List<TextLine> RemovedLines;
        internal List<TextAnchor> RemovedAnchors;
        public void AddRemovedLine(TextLine line)
        {
            if (RemovedLines == null)
                RemovedLines = new List<TextLine>();
            RemovedLines.Add(line);
        }

        public void AddRemovedAnchor(TextAnchor anchor)
        {
            if (RemovedAnchors == null)
                RemovedAnchors = new List<TextAnchor>();
            RemovedAnchors.Add(anchor);
        }

        public void RaiseEvents()
        {
            if (RemovedAnchors != null)
            {
                RemovedAnchors.ForEach(a => a.RaiseDeleted());
            }
        }
    }
}
