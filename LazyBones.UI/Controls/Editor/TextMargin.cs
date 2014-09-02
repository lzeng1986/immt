using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.Editor
{
    //文本区域，用于绘制文本内容
    class TextMargin : MarginBase
    {
        public TextMargin(TextArea textArea)
            : base(textArea)
        {
        }
        public EditorOptions EditorSetting
        {
            get { return textArea.Document.EditorOptions; }
        }
        public int FirstVisibleLine
        {
            get { return textArea.Document.GetFirstLogicalLine(FirstPhysicalLine); }
            set { textArea.VisualLocation = new Point(textArea.VisualLocation.X, textArea.Document.GetVisibleLine(value) * textArea.TextMeasurer.FontHeight); }
        }
        public int FirstPhysicalLine
        {
            get { return textArea.VisualLocation.Y / textArea.TextMeasurer.FontHeight; }
        }
        public int LineHeightRemainder
        {
            get { return textArea.VisualLocation.Y % textArea.TextMeasurer.FontHeight; }
        }
        public int VisibleLineDrawingRemainder
        {
            get { return textArea.VisualLocation.Y % textArea.TextMeasurer.FontHeight; }
        }
        public int VisibleLineCount
        {
            get { return 1 + DisplayBounds.Height / textArea.TextMeasurer.FontHeight; }
        }
        public int VisibleColumnCount
        {
            get { return (DisplayBounds.Width / textArea.TextMeasurer.WideSpaceWidth) - 1; }
        }

        public Document Document
        {
            get { return textArea.Document; }
        }
        public FoldMarker GetFoldMarkerAt(int yPos, int xPos)//查看当前坐标位置是否有折叠的文本
        {
            var lineNo = GetLogicalLineNo(yPos);
            var marker = Document.FoldingManager.GetFoldedFoldingsWithStart(lineNo).FirstOrDefault();
            if (marker == null)
                return null;
            var textLine = Document.GetLineByLineNo(lineNo);
            using (var g = textArea.CreateGraphics())
            {
                var width = textArea.TextMeasurer.MeasureLogicalLineWidth(g, 0, textLine.Length, yPos);
                var foldWidth = textArea.TextMeasurer.MeasureStringWidth(g, marker.FoldText, EditorSetting.Font.RegularFont);
                if (width < xPos && xPos < (width + foldWidth))
                    return marker;
                return null;
            }
        }
        public FoldMarker GetFoldMarkerAt(Point mousePosition)
        {
            return GetFoldMarkerAt(mousePosition.Y, mousePosition.X);
        }
        public TextLocation PointToLocation(int yPos, int xPos)//将坐标位置转换成文档位置TextLocation
        {
            yPos = GetLogicalLineNo(yPos);
            xPos += textArea.VisualLocation.X;
            if (xPos <= 0)
                return new TextLocation(0, yPos);
            return new TextLocation(xPos / textArea.TextMeasurer.WideSpaceWidth, yPos);
        }
        public TextLocation PointToLocation(Point mousePosition)
        {
            return PointToLocation(mousePosition.Y, mousePosition.X);
        }
        public int GetLogicalLineNo(int visualPosY)
        {
            int clickedVisualLine = Math.Max(0, (visualPosY + textArea.VisualLocation.Y) / textArea.TextMeasurer.FontHeight);
            return textArea.Document.GetFirstLogicalLine(clickedVisualLine);
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            TextWord.Space.SyntaxColor = textArea.Document.TextDrawInfoManager["SpaceMarkers"];
            TextWord.Tab.SyntaxColor = textArea.Document.TextDrawInfoManager["TabMarkers"];

            int xDelta = textArea.VisualLocation.X;
            if (xDelta > 0)
            {
                g.SetClip(DisplayBounds);
            }
            var visibleLineCount = (DisplayBounds.Height + VisibleLineDrawingRemainder) / textArea.TextMeasurer.FontHeight + 1;
            var lineRectangle = new Rectangle(DisplayBounds.X - xDelta,
                                                        DisplayBounds.Top - VisibleLineDrawingRemainder,
                                                        DisplayBounds.Width + xDelta,
                                                        textArea.TextMeasurer.FontHeight
                                                        );
            var firstLineNo = textArea.Document.GetVisibleLine(FirstVisibleLine);
            //绘制行
            foreach (var y in Enumerable.Range(0, visibleLineCount))
            {
                if (rect.IntersectsWith(lineRectangle))
                {
                    var currentLine = textArea.Document.GetFirstLogicalLine(firstLineNo + y);
                    DrawLine(g, currentLine, lineRectangle);
                    if (currentLine == textArea.Caret.Line && textArea.EditorOptions.HighlightCurrentLine)//绘制高亮显示当前行
                        g.DrawRectangle(Pens.WhiteSmoke, DisplayBounds.X, lineRectangle.Y,
                            DisplayBounds.Width - 1, lineRectangle.Height - 1);
                }
                lineRectangle.Y += textArea.TextMeasurer.FontHeight;
            }

            DrawTextMarker(g);

            if (xDelta > 0)
            {
                g.ResetClip();
            }
            textArea.Caret.Paint(g);
        }
        private struct MarkerToDraw
        {
            internal TextMarker marker;
            internal RectangleF drawingRect;
            public MarkerToDraw(TextMarker marker, RectangleF drawingRect)
            {
                this.marker = marker;
                this.drawingRect = drawingRect;
            }
        }
        List<MarkerToDraw> markersToDraw = new List<MarkerToDraw>();
        void AddTextMarkerToDraw(TextMarker marker, RectangleF drawingRect)
        {
            markersToDraw.Add(new MarkerToDraw(marker, drawingRect));
        }
        void DrawTextMarker(Graphics g)//绘制文本标记，如下划线、波浪线等
        {
            foreach (var m in markersToDraw)
            {
                var marker = m.marker;
                var rect = m.drawingRect;
                var y = (int)rect.Bottom - 1;
                switch (marker.Type)
                {
                    case TextMarkerType.Underline://下划线
                        g.DrawLine(BrushPool.GetPen(marker.Color), rect.Left, y, rect.Right, y);
                        break;
                    case TextMarkerType.WaveLine://波浪线
                        int reminder = ((int)rect.Left) % 6;
                        for (var i = (int)rect.Left - reminder; i < rect.Right; i += 6)
                        {
                            g.DrawLine(BrushPool.GetPen(marker.Color), i, y - 1, i + 3, y - 3);
                            if (i + 3 < rect.Right)
                                g.DrawLine(BrushPool.GetPen(marker.Color), i + 3, y - 3, i + 6, y - 1);
                        }
                        break;
                }
            }
            markersToDraw.Clear();
        }

        static void DrawString(Graphics g, string text, Font font, Color color, int x, int y)
        {
            TextRenderer.DrawText(g, text, font, new Point(x, y), color, TextMeasurer.MeasureTextFormat);
        }
        void DrawSpaceMarker(Graphics g, Color color, int x, int y)
        {
            var spaceMarkerColor = textArea.Document.TextDrawInfoManager["SpaceMarkers"];
            DrawString(g, "\u00B7", EditorSetting.Font[spaceMarkerColor], color, x, y);
        }
        void DrawTabMarker(Graphics g, Color color, int x, int y)
        {
            var tabMarkerColor = textArea.Document.TextDrawInfoManager["TabMarkers"];
            DrawString(g, "\u00BB", EditorSetting.Font[tabMarkerColor], color, x, y);
        }
        int DrawEOLMarker(Graphics g, Color color, Brush backBrush, int x, int y)
        {
            var eolMarkerColor = textArea.Document.TextDrawInfoManager["EOLMarkers"];
            int width = textArea.TextMeasurer.GetWidth('\u00B6', EditorSetting.Font[eolMarkerColor]);
            g.FillRectangle(backBrush, new RectangleF(x, y, width, textArea.TextMeasurer.FontHeight));
            DrawString(g, "\u00B6", EditorSetting.Font[eolMarkerColor], color, x, y);
            return width;
        }
        int DrawText(Graphics g, string word, int x, int y, Font font, Color foreColor, Brush backBrush)
        {
            if (string.IsNullOrEmpty(word))
                return 0;
            if (word.Length > TextMeasurer.MaxWordLength)
            {
                int width = 0;
                for (int i = 0; i < word.Length; i += TextMeasurer.MaxWordLength)
                {
                    var offset = 0;
                    if (i + TextMeasurer.MaxWordLength < word.Length)
                        offset += DrawText(g, word.Substring(i, TextMeasurer.MaxWordLength), x, y, font, foreColor, backBrush);
                    else
                        offset += DrawText(g, word.Substring(i, word.Length - i), x, y, font, foreColor, backBrush);
                    x += offset;
                    width += offset;
                }
                return width;
            }
            var wordWidth = textArea.TextMeasurer.MeasureStringWidth(g, word, font);
            if (backBrush != null)
                g.FillRectangle(backBrush, new Rectangle(x, y, wordWidth + 1, textArea.TextMeasurer.FontHeight));
            TextRenderer.DrawText(g, word, font, new Point(x, y), foreColor, TextMeasurer.MeasureTextFormat);
            return wordWidth;
        }
        void DrawVerticalRuler(Graphics g, Rectangle lineRectangle)
        {
            int xpos = textArea.TextMeasurer.WideSpaceWidth * EditorSetting.VerticalRulerCol - textArea.VisualLocation.X;
            if (xpos <= 0)
                return;
            var vRulerColor = textArea.Document.TextDrawInfoManager["VRuler"];
            g.DrawLine(BrushPool.GetPen(vRulerColor.BackColor), DisplayBounds.Left + xpos, lineRectangle.Top, DisplayBounds.Left + xpos, lineRectangle.Bottom);
        }
        int DrawFoldingText(Graphics g, int lineNumber, int physicalXPos, Rectangle lineRectangle, string text, bool drawSelected)
        {
            var selectionColor = textArea.Document.TextDrawInfoManager["Selection"];
            var defaultColor = textArea.Document.TextDrawInfoManager["Default"];
            var backgroundBrush = BrushPool.GetBrush(drawSelected ? selectionColor.BackColor : defaultColor.BackColor);
            var font = EditorSetting.Font.RegularFont;

            var wordWidth = textArea.TextMeasurer.MeasureStringWidth(g, text, font) + TextMeasurer.AdditionalFoldTextSize;
            var rect = new Rectangle(physicalXPos, lineRectangle.Y, wordWidth, lineRectangle.Height - 1);

            g.FillRectangle(backgroundBrush, rect);

            DrawString(g, text, font, drawSelected ? selectionColor.ForeColor : defaultColor.ForeColor, rect.X + 1, rect.Y);
            g.DrawRectangle(BrushPool.GetPen(drawSelected ? Color.DarkGray : Color.Gray), rect);
            return physicalXPos + wordWidth + 1;
        }
        //绘制文本行，只支持纵向折叠，不支持横向折叠
        void DrawLine(Graphics g, int logicalLineNo, Rectangle lineRectangle)
        {
            var bgColorBrush = BrushPool.GetBrush(textArea.Document.TextDrawInfoManager["Default"].BackColor);
            var backgroundBrush = textArea.Enabled ? bgColorBrush : SystemBrushes.InactiveBorder;

            if (logicalLineNo >= textArea.Document.TotalOfLine)
            {
                g.FillRectangle(backgroundBrush, lineRectangle);
                if (EditorSetting.VerticalRulerVisible)
                    DrawVerticalRuler(g, lineRectangle);
                return;
            }
            int physicalXPos = lineRectangle.X;
            var currentLine = textArea.Document.GetLineByLineNo(logicalLineNo);
            physicalXPos = DrawLineText(g, logicalLineNo, lineRectangle, physicalXPos);
            if (EditorSetting.EnableFolding)
            {
                var firstFolding = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(logicalLineNo).FirstOrDefault();
                if (firstFolding != null)
                {
                    var selection = textArea.SelectionManager.GetSelection(currentLine.Offset);
                    var drawSelected = selection != null && (selection.Start.Line == firstFolding.Start.Line + 1) && selection.End == firstFolding.End;
                    physicalXPos = DrawFoldingText(g, logicalLineNo, physicalXPos, lineRectangle, firstFolding.FoldText, drawSelected);
                }
            }
            if (logicalLineNo < textArea.Document.TotalOfLine)
            {
                if (EditorSetting.EOLMarkerVisible)
                {
                    var eolMarkerColor = textArea.Document.TextDrawInfoManager["EOLMarkers"];
                    physicalXPos += DrawEOLMarker(g, eolMarkerColor.BackColor, backgroundBrush, physicalXPos, lineRectangle.Y);
                }
                g.FillRectangle(backgroundBrush,
                                new RectangleF(physicalXPos,
                                    lineRectangle.Y,
                                    lineRectangle.Right - physicalXPos,
                                    lineRectangle.Height));
            }
            if (EditorSetting.VerticalRulerVisible)
                DrawVerticalRuler(g, lineRectangle);

        }
        //关键函数，绘制文本
        int DrawLineText(Graphics g, int lineNo, Rectangle lineRect, int physicalXPos)
        {
            var line = textArea.Document.GetLineByLineNo(lineNo);
            if (line.Words.Count == 0)
                return physicalXPos;

            var selectionColor = textArea.Document.TextDrawInfoManager["Selection"];
            var selectionRange = textArea.SelectionManager.GetSelectionInLine(lineNo);
            var selectionBackBrush = BrushPool.GetBrush(selectionColor.BackColor);

            int wordStartOffset = 0;
            foreach (var word in line.Words)
            {
                if (physicalXPos >= lineRect.Right)
                    break;

                int wordEndOffset = wordStartOffset + word.Length - 1;
                Brush wordBackBrush = BrushPool.GetBrush(word.SyntaxColor.BackColor);
                var font = EditorSetting.Font[word.SyntaxColor];
                //绘制时需要考虑行是否处于选择状态，需要把处于选择状态的部分用selectionBackBrush绘制
                if (selectionRange == ColumnRange.WholeColumn || (selectionRange.StartColumn <= wordStartOffset && wordEndOffset < selectionRange.EndColumn))
                {
                    physicalXPos = DrawTextWord(g, word, lineRect, physicalXPos, selectionBackBrush);
                }
                else if (wordStartOffset < selectionRange.StartColumn && selectionRange.StartColumn <= wordEndOffset)
                {
                    var delta = selectionRange.StartColumn - wordStartOffset;
                    var width = DrawText(g, Document.GetText(line.Offset + wordStartOffset, delta), physicalXPos, lineRect.Y,
                        font, word.Color, wordBackBrush);
                    physicalXPos += width;
                    width = DrawText(g, Document.GetText(line.Offset + selectionRange.StartColumn, word.Length - delta), physicalXPos, lineRect.Y,
                        font, word.Color, selectionBackBrush);
                    physicalXPos += width;
                }
                else if (wordStartOffset < selectionRange.EndColumn && selectionRange.EndColumn <= wordEndOffset)
                {
                    var delta = wordEndOffset - selectionRange.EndColumn;
                    var width = DrawText(g, Document.GetText(line.Offset + wordStartOffset, delta), physicalXPos, lineRect.Y,
                        font, word.Color, selectionBackBrush);
                    physicalXPos += width;
                    width = DrawText(g, Document.GetText(line.Offset + selectionRange.StartColumn, word.Length - delta), physicalXPos, lineRect.Y,
                        font, word.Color, wordBackBrush);
                    physicalXPos += width;
                }
                else
                {
                    physicalXPos = DrawTextWord(g, word, lineRect, physicalXPos, wordBackBrush);
                }
                wordStartOffset += word.Length;
            }
            if (physicalXPos < lineRect.Right)
            {
                var markers = Document.TextMarkerManager.GetMarkers(line.Offset, line.Length);
                foreach (var marker in markers)
                {
                    AddTextMarkerToDraw(marker, new RectangleF(physicalXPos, lineRect.Y, textArea.TextMeasurer.WideSpaceWidth, lineRect.Height));
                }
            }
            return physicalXPos;
        }
        int DrawTextWord(Graphics g, TextWord word, Rectangle lineRect, int physicalXPos, Brush wordBackBrush)
        {
            if (word.Type == WordType.Space)
            {
                var wordRectangle = new RectangleF(physicalXPos, lineRect.Y, textArea.TextMeasurer.SpaceWidth, lineRect.Height);
                g.FillRectangle(wordBackBrush, wordRectangle);

                if (EditorSetting.WhiteSpaceVisible)
                    DrawSpaceMarker(g, word.Color, physicalXPos, lineRect.Y);

                return physicalXPos + textArea.TextMeasurer.SpaceWidth;
            }
            if (word.Type == WordType.Tab)
            {
                var wordRectangle = new RectangleF(physicalXPos, lineRect.Y, textArea.TextMeasurer.WideSpaceWidth * EditorSetting.TabIndent, lineRect.Height);
                g.FillRectangle(wordBackBrush, wordRectangle);

                if (EditorSetting.TabVisible)
                    DrawTabMarker(g, word.Color, physicalXPos, lineRect.Y);

                return physicalXPos + textArea.TextMeasurer.WideSpaceWidth * EditorSetting.TabIndent;
            }
            var wordWidth = DrawText(g, word.Text, physicalXPos, lineRect.Y, EditorSetting.Font[word.SyntaxColor], word.Color, wordBackBrush);
            return physicalXPos + wordWidth;
        }

        bool clickedOnSelectedText = false;
        Point mouseDownPos;
        public override void MouseDown(MouseEventArgs e)
        {
            mouseDownPos = e.Location;
            if (e.Button != MouseButtons.Left)
                return;

            var posInTextArea = e.Location;
            posInTextArea.Offset(-DisplayBounds.X, -DisplayBounds.Y);
            var marker = GetFoldMarkerAt(posInTextArea);
            if (marker != null && marker.Folded)
            {
                textArea.SelectionManager.SelectionStart = marker.Start;
                textArea.SelectionManager.SetSelection(new Selection(textArea.Document, marker.Start, marker.End));
                textArea.Focus();
                return;
            }
            var realmousepos = textArea.MouseLocation;
            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                clickedOnSelectedText = false;
                if (textArea.SelectionManager.HasSelection)
                    textArea.SelectionManager.ExtendSelectionTo(realmousepos);
            }
            else
            {
                textArea.SelectionManager.ClearSelection();
                textArea.SelectionManager.SelectionStart = realmousepos;
                int offset = textArea.Document.LocationToOffset(realmousepos);
                clickedOnSelectedText = textArea.SelectionManager.IsSelectedAt(offset);
            }
            textArea.Caret.Location = realmousepos;
            textArea.SetDesiredColumn();
        }
        public override void MouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (clickedOnSelectedText)
                textArea.SelectionManager.ExtendSelectionTo(textArea.MouseLocation);
            else if(textArea.SelectionManager.HasSelection)
            {
                if (Math.Abs(mouseDownPos.X - e.X) >= SystemInformation.DragSize.Width / 2 ||
                    Math.Abs(mouseDownPos.Y - e.Y) >= SystemInformation.DragSize.Height / 2)
                {
                    clickedOnSelectedText = false;
                    var selection = textArea.SelectionManager.GetSelection(textArea.Caret.Offset);
                    if (selection != null)
                    {
                        string text = selection.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            DataObject dataObject = new DataObject();
                            dataObject.SetData(DataFormats.UnicodeText, true, text);
                            dataObject.SetData(selection);
                            textArea.DoDragDrop(dataObject, selection.ReadOnly ? DragDropEffects.All & ~DragDropEffects.Move : DragDropEffects.All);
                        }
                    }
                }
            }
        }
        public override void MouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            textArea.SelectionManager.ClearSelection();
            var marker = textArea.TextMargin.GetFoldMarkerAt(e.Location - textArea.TextMargin.DisplayBounds.Size);
            if (marker != null && marker.Folded)//如果当前代码折叠，则展开
            {
                marker.Folded = false;
                textArea.TextEditor.AdjustScrollBars();
            }
            //if (textArea.Caret.Offset < textArea.Document.Length)
            //{
            //    switch (textArea.Document.GetChar(textArea.Caret.Offset))
            //    {
            //        case '"':
            //            if (textArea.Caret.Offset < textArea.Document.Length)
            //            {
            //                int next = textArea.Document.FindNext(textArea.Caret.Offset + 1, '"');
            //                minSelection = textArea.Caret.Position;
            //                if (next > textArea.Caret.Offset && next < textArea.Document.Length)
            //                    next += 1;
            //                maxSelection = textArea.Document.OffsetToLocation(next);
            //            }
            //            break;
            //        default:
            //            minSelection = textArea.Document.OffsetToLocation(textArea.Document.FindWordStart(textArea.Caret.Offset));
            //            maxSelection = textArea.Document.OffsetToLocation(textArea.Document.FindWordEnd(textArea.Caret.Offset));
            //            break;

            //    }
            //    textArea.Caret.Position = maxSelection;
            //    //textArea.SelectionManager.ExtendSelectionTo(minSelection, maxSelection);
            //}

            //if (textArea.SelectionManager.HasSelection)
            //{
            //    var selection = textArea.SelectionManager.Selections[0];

            //    selection.Start = minSelection;
            //    selection.End = maxSelection;
            //    textArea.SelectionManager.SelectionStart = minSelection;
            //}

            // after a double-click selection, the caret is placed correctly,
            // but it is not positioned internally.  The effect is when the cursor
            // is moved up or down a line, the caret will take on the column first
            // clicked on for the double-click
            textArea.SetDesiredColumn();

            // HACK WARNING !!!
            // must refresh here, because when a error tooltip is showed and the underlined
            // code is double clicked the textArea don't update corrctly, updateline doesn't
            // work ... but the refresh does.
            // Mike
            textArea.Refresh();
        }
    }
}
