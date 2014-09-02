using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 用于显示代码折叠
    /// </summary>
    class FoldMargin : MarginBase
    {
        public FoldMargin(TextArea textArea)
            : base(textArea)
        {
        }
        public override bool Visible
        {
            get { return textArea.EditorOptions.EnableFolding; }
        }
        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                return;
            var foldLineColor = textArea.Document.TextDrawInfoManager["FoldLine"];
            var markerRectangle = new Rectangle(DisplayBounds.X,
                                                          DisplayBounds.Top + textArea.TextMargin.VisibleLineDrawingRemainder,
                                                          DisplayBounds.Width,
                                                          textArea.TextMeasurer.FontHeight);

            foreach (var i in Enumerable.Range(0, textArea.TextMargin.VisibleLineCount))
            {
                g.FillRectangle(BrushPool.GetBrush(foldLineColor.BackColor), markerRectangle);
                if (rect.IntersectsWith(markerRectangle))
                {
                    int currentLine = textArea.Document.GetFirstLogicalLine(textArea.TextMargin.FirstPhysicalLine + i);
                    if (currentLine < textArea.Document.TotalOfLine)
                        PaintFoldMarker(g, currentLine, markerRectangle);
                }
                markerRectangle.Y += textArea.TextMeasurer.FontHeight;
            }
        }

        int selectedFoldLine;
        void PaintFoldMarker(Graphics g, int lineNo, Rectangle foldRect)
        {
            var foldLineColor = textArea.Document.TextDrawInfoManager["FoldLine"];
            var selectedFoldLineColor = textArea.Document.TextDrawInfoManager["SelectedFoldLine"];

            var foldingsWithStart = textArea.Document.FoldingManager.GetFoldingsWithStart(lineNo);
            var foldingsBetween = textArea.Document.FoldingManager.GetFoldingsContainsLineNo(lineNo);
            var foldingsWithEnd = textArea.Document.FoldingManager.GetFoldingsWithEnd(lineNo);

            var isFoldStart = foldingsWithStart.Any();
            var isBetween = foldingsBetween.Any();
            var isFoldEnd = foldingsWithEnd.Any();

            bool isStartSelected = foldingsWithStart.Any(l => l.Start.Line == selectedFoldLine);
            bool isBetweenSelected = foldingsBetween.Any(l => l.Start.Line == selectedFoldLine);
            bool isEndSelected = foldingsWithEnd.Any(l => l.Start.Line == selectedFoldLine);

            int foldMarkerSize = (int)(textArea.TextMeasurer.FontHeight * 0.57);
            foldMarkerSize -= (foldMarkerSize) % 2;
            int markerXPos = foldRect.X + (int)((foldRect.Width - foldMarkerSize) / 2);
            int markerYPos = foldRect.Y + (int)((foldRect.Height - foldMarkerSize) / 2);
            int xPos = foldRect.X + (foldRect.Width - foldMarkerSize) / 2 + foldMarkerSize / 2;
            int yPos = foldRect.Y + (foldRect.Height - foldMarkerSize) / 2 + foldMarkerSize / 2;

            var linePen = BrushPool.GetPen(foldLineColor.BackColor);
            var selectedLinePen = BrushPool.GetPen(selectedFoldLineColor.ForeColor);
            var selectedLineBrush = BrushPool.GetBrush(selectedFoldLineColor.BackColor);

            if (isFoldStart)
            {
                var isOpened = foldingsWithStart.Any(f => f.Folded);

                g.DrawRectangle(linePen, markerXPos, markerYPos, foldMarkerSize, foldMarkerSize);

                var space = foldMarkerSize / 8 + 1;
                g.DrawLine(linePen, markerXPos + space, yPos, markerXPos + foldMarkerSize - space, yPos);

                if (!isOpened)
                    g.DrawLine(linePen, xPos, markerYPos + space, xPos, markerYPos + foldMarkerSize - space);
            }
            else if (isFoldEnd)
            {
                int midy = foldRect.Top + foldRect.Height / 2;

                // draw fold end marker
                g.DrawLine(BrushPool.GetPen(isEndSelected ? selectedFoldLineColor.ForeColor : foldLineColor.ForeColor),
                           xPos,
                           midy,
                           xPos + foldMarkerSize / 2,
                           midy);

                // draw line above fold end marker
                // must be drawn after fold marker because it might have a different color than the fold marker
                g.DrawLine(BrushPool.GetPen(isBetweenSelected || isEndSelected ? selectedFoldLineColor.ForeColor : foldLineColor.ForeColor),
                           xPos,
                           foldRect.Top,
                           xPos,
                           midy);

                // draw line below fold end marker
                if (isBetween)
                {
                    g.DrawLine(BrushPool.GetPen(isBetweenSelected ? selectedFoldLineColor.ForeColor : foldLineColor.ForeColor),
                               xPos,
                               midy + 1,
                               xPos,
                               foldRect.Bottom);
                }
            }
            else if (isBetween)
            {
                if (isBetweenSelected)
                    g.FillRectangle(selectedLineBrush,foldRect);
                g.DrawLine(linePen, xPos, foldRect.Top, xPos, foldRect.Bottom);
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            int physicalLine = +(int)((e.Y + textArea.VisualLocation.Y) / textArea.TextMeasurer.FontHeight);
            int realline = textArea.Document.GetFirstLogicalLine(physicalLine);

            // focus the textarea if the user clicks on the line number view
            textArea.Focus();

            if (!Visible || realline < 0 || realline + 1 >= textArea.Document.TotalOfLine)
                return;

            var foldMarkers = textArea.Document.FoldingManager.GetFoldingsWithStart(realline);
            foreach (FoldMarker fm in foldMarkers)
                fm.Folded = !fm.Folded;

            textArea.Document.FoldingManager.RaiseFoldingsChanged(EventArgs.Empty);
        }
        public override void MouseLeave(EventArgs e)
        {
            if (selectedFoldLine == -1)
                return;
            selectedFoldLine = -1;
            textArea.Refresh(this);
        }
    }
}
