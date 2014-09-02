using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 管理代码折叠
    /// </summary>
    public class FoldingManager
    {
        List<FoldMarker> foldMarkers = new List<FoldMarker>();
        List<FoldMarker> foldMarkersByEnd = new List<FoldMarker>();
        Document doc;

        public FoldingManager(Document doc, TextLineManager lineManager)
        {
            this.doc = doc;
            lineManager.LineCountChanged += lineManager_LineCountChanged;
        }

        void lineManager_LineCountChanged(object sender, LineCountChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        public IFoldStrategy FoldStrategy { get; set; }

        public IEnumerable<FoldMarker> GetFoldingsFromLocation(int line, int column)
        {
            return GetFoldingsFromLocation(new TextLocation(column, line));
        }
        public IEnumerable<FoldMarker> GetFoldingsFromLocation(TextLocation location)
        {
            return foldMarkers.Where(f => f.Start <= location && location <= f.End);
        }
        public IEnumerable<FoldMarker> TopLevelFoldedFoldings
        {
            get
            {
                var end = 0;
                foreach (var fm in foldMarkers.Where(f => f.Folded))
                {
                    if (fm.Start.Line >= end)
                    {
                        yield return fm;
                        end = fm.End.Line;
                    }
                }
            }
        }
        public IEnumerable<FoldMarker> GetFoldingsWithStart(int lineNo)
        {
            return GetFoldingsByStartAfterColumn(lineNo, -1, false);
        }

        public IEnumerable<FoldMarker> GetFoldedFoldingsWithStart(int lineNo)
        {
            return GetFoldingsByStartAfterColumn(lineNo, -1, true);
        }

        public IEnumerable<FoldMarker> GetFoldedFoldingsWithStartAfterColumn(int lineNo, int column)
        {
            return GetFoldingsByStartAfterColumn(lineNo, column, true);
        }
        IEnumerable<FoldMarker> GetFoldingsByStartAfterColumn(int lineNo, int column, bool forceFolded)
        {
            var index = foldMarkers.BinarySearch(new FoldMarker(doc, lineNo, column, lineNo, column),
                    FoldMarkerComparer.ByStart);
            if (index < 0)
                index = ~index;
            while (index < foldMarkers.Count)
            {
                var fm = foldMarkers[index];
                if (fm.Start.Line > lineNo)
                    yield break;
                if (fm.Start.Column > column && (!forceFolded || fm.Folded))
                    yield return fm;
                index++;
            }
        }
        IEnumerable<FoldMarker> GetFoldingsByEndAfterColumn(int lineNo, int column, bool forceFolded)
        {
            var index = foldMarkersByEnd.BinarySearch(new FoldMarker(doc, lineNo, column, lineNo, column),
                    FoldMarkerComparer.ByEnd);
            if (index < 0)
                index = ~index;
            while (index < foldMarkers.Count)
            {
                var fm = foldMarkers[index];
                if (fm.End.Line > lineNo)
                    yield break;
                if (fm.End.Column > column && (!forceFolded || fm.Folded))
                    yield return fm;
                index++;
            }
        }

        public IEnumerable<FoldMarker> GetFoldingsWithEnd(int lineNo)
        {
            return GetFoldingsByEndAfterColumn(lineNo, -1, false);
        }

        public IEnumerable<FoldMarker> GetFoldedFoldingsWithEnd(int lineNo)
        {
            return GetFoldingsByEndAfterColumn(lineNo, -1, true);
        }
        public bool IsFoldStart(int lineNo)
        {
            return GetFoldingsWithStart(lineNo).Any();
        }
        public bool IsFoldEnd(int lineNo)
        {
            return GetFoldingsWithEnd(lineNo).Any();
        }
        public IEnumerable<FoldMarker> GetFoldingsContainsLineNo(int lineNo)
        {
            return foldMarkers.Where(fm => fm.Start.Line < lineNo && lineNo < fm.End.Line);
        }
        public bool IsBetweenFolding(int lineNo)
        {
            return foldMarkers.Any(fm => fm.Start.Line < lineNo && lineNo < fm.End.Line);
        }
        public bool IsLineVisible(int lineNo)
        {
            return !foldMarkers.Where(fm => fm.Start.Line < lineNo && lineNo < fm.End.Line).Any(fm => fm.Folded);
        }
        public void UpdateFoldings(string fileName, object parseInfo)
        {
            UpdateFoldings(FoldStrategy.GenerateFoldMarkers(doc, fileName, parseInfo));
        }
        public void UpdateFoldings(List<FoldMarker> newFoldings)
        {
            int oldFoldingsCount = foldMarkers.Count;
            lock (this)
            {
                if (newFoldings == null)
                {
                    foldMarkers.Clear();
                    foldMarkersByEnd.Clear();
                }
                else
                {
                    if (newFoldings.Count != 0)
                    {
                        newFoldings.Sort(FoldMarkerComparer.ByStart);
                        if (foldMarkers.Count == newFoldings.Count)
                        {
                            for (int i = 0; i < foldMarkers.Count; ++i)
                            {
                                newFoldings[i].Folded = foldMarkers[i].Folded;
                            }
                            foldMarkers = newFoldings;
                        }
                        else
                        {
                            var newInd = 0;
                            var oldInd = 0;
                            while (newInd < newFoldings.Count && oldInd < foldMarkers.Count)
                            {
                                var n = newFoldings[newInd].CompareTo(foldMarkers[oldInd]);
                                if (n > 0)
                                {
                                    oldInd++;
                                }
                                else
                                {
                                    if (n == 0)
                                        newFoldings[newInd].Folded = foldMarkers[oldInd].Folded;
                                    newInd++;
                                }
                            }
                        }
                    }
                    foldMarkers = newFoldings;
                    foldMarkersByEnd = new List<FoldMarker>(newFoldings);
                    foldMarkersByEnd.Sort(FoldMarkerComparer.ByEnd);
                }
            }
            if (oldFoldingsCount != foldMarkers.Count)
            {
                doc.RequestUpdate(new TextAreaUpdater(TextAreaUpdateType.WholeTextArea));
                doc.RaiseUpdateCommited();
            }
        }

        public string SerializeToString()
        {
            var sb = new StringBuilder();
            foreach (var marker in foldMarkers)
            {
                sb.Append(marker.Offset); sb.Append("\n");
                sb.Append(marker.Length); sb.Append("\n");
                sb.Append(marker.FoldText); sb.Append("\n");
                sb.Append(marker.Folded); sb.Append("\n");
            }
            return sb.ToString();
        }

        public void DeserializeFromString(string str)
        {
            try
            {
                string[] lines = str.Split('\n');
                for (int i = 0; i < lines.Length && lines[i].Length > 0; i += 4)
                {
                    int offset = Convert.ToInt32(lines[i]);
                    int length = Convert.ToInt32(lines[i + 1]);
                    string text = lines[i + 2];
                    bool folded = Convert.ToBoolean(lines[i + 3]);
                    var marker = foldMarkers.FirstOrDefault(fm => fm.Offset == offset && fm.Length == length);
                    if (marker == null)
                        foldMarkers.Add(new FoldMarker(doc, offset, length, text, folded));
                    else
                        marker.Folded = folded;

                }
                if (lines.Length > 0)
                {
                    RaiseFoldingsChanged(EventArgs.Empty);
                }
            }
            catch (Exception)
            {
            }
        }
        public event EventHandler FoldingsChanged;
        public void RaiseFoldingsChanged(EventArgs e)
        {
            if (FoldingsChanged != null)
                FoldingsChanged(this, e);
        }
    }
}
