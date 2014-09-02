using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace LazyBones.UI.Controls.TextEditor
{
    //测量文本
    class TextMeasurer
    {
        public const TextFormatFlags MeasureTextFormat = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping;
        public const int MaxWordLength = 1000;
        const int MaxCacheSize = 2000;
        const int MinTabWidth = 4;
        public const int AdditionalFoldTextSize = 1;

        TextArea textArea;
        Document document;
        public TextMeasurer(TextArea textArea)
        {
            this.textArea = textArea;
            document = textArea.Document;
        }

        public int SpaceWidth { get; private set; }
        public int WideSpaceWidth { get; private set; }

        Dictionary<PairKey<Font, string>, int> measureCache = new Dictionary<PairKey<Font, string>, int>();
        public int MeasureStringWidth(Graphics g, string word, Font font)//测量字符串宽度
        {
            if (string.IsNullOrEmpty(word))
                return 0;
            int width;
            if (word.Length > MaxWordLength)
            {
                width = 0;
                for (int i = 0; i < word.Length; i += MaxWordLength)
                {
                    if (i + MaxWordLength < word.Length)
                        width += MeasureStringWidth(g, word.Substring(i, MaxWordLength), font);
                    else
                        width += MeasureStringWidth(g, word.Substring(i, word.Length - i), font);
                }
                return width;
            }
            var key = new PairKey<Font, string>(font, word);
            if (measureCache.TryGetValue(key, out width))
                return width;
            if (measureCache.Count > MaxCacheSize)
            {
                measureCache.Clear();
            }

            width = TextRenderer.MeasureText(g, word, font, new Size(short.MaxValue, short.MaxValue), MeasureTextFormat).Width;
            measureCache.Add(key, width);
            return width;
        }

        public static bool IsNearerToAThanB(int num, int a, int b)
        {
            return Math.Abs(a - num) < Math.Abs(b - num);
        }

        Dictionary<PairKey<Font, char>, int> fontBoundCharWidthCache = new Dictionary<PairKey<Font, char>, int>();
        public int GetWidth(char ch, Font font)//测量字符宽度
        {
            var key = new PairKey<Font, char>(font, ch);
            int width = 0;
            if (!fontBoundCharWidthCache.TryGetValue(key, out width))
            {
                using (Graphics g = textArea.CreateGraphics())
                {
                    width = TextRenderer.MeasureText(g, ch.ToString(), font, new Size(short.MaxValue, short.MaxValue), MeasureTextFormat).Width;
                }
                fontBoundCharWidthCache.Add(key, width);
            }
            return width;
        }
        public int GetWidth(Graphics g, char ch, Font font)//测量字符宽度
        {
            var key = new PairKey<Font, char>(font, ch);
            int width = 0;
            if (!fontBoundCharWidthCache.TryGetValue(key, out width))
            {
                width = TextRenderer.MeasureText(g, ch.ToString(), font, new Size(short.MaxValue, short.MaxValue), MeasureTextFormat).Width;
                fontBoundCharWidthCache.Add(key, width);
            }
            return width;
        }
        public int MeasureLogicalLineWidth(Graphics g, int start, int end, int logicalLineNo)//测量一行文本的像素宽度
        {
            if (start > end)
                throw new ArgumentException("start > end");
            if (start == end)
                return 0;
            int drawingPos = 0;
            int tabWidth = document.EditorOptions.TabIndent * WideSpaceWidth;
            var line = document.GetLineByLineNo(logicalLineNo);
            int wordOffset = 0;
            var fontContainer = document.EditorOptions.Font;
            foreach (var word in line.Words)
            {
                if (wordOffset >= end)
                    break;
                if (wordOffset + word.Length >= start)
                {
                    switch (word.Type)
                    {
                        case WordType.Space:
                            drawingPos += SpaceWidth;
                            break;
                        case WordType.Tab:
                            drawingPos += tabWidth;
                            break;
                        case WordType.Word:
                            var wordStart = Math.Max(wordOffset, start);
                            var wordLength = Math.Min(wordOffset + word.Length, end) - wordStart;
                            var text = document.GetText(line.Offset + wordStart, wordLength);
                            drawingPos += MeasureStringWidth(g, text, fontContainer[word.SyntaxColor]);
                            break;
                    }
                }
                wordOffset += word.Length;
            }
            if (end > line.Length)
                drawingPos += WideSpaceWidth * (end - line.Length);
            return drawingPos;
        }
        public int GetVisualColumnFast(TextLine line, int logicalColumn)
        {
            int lineOffset = line.Offset;
            int tabIndent = document.EditorOptions.TabIndent;
            var tabCount = document.GetText(lineOffset, Math.Min(logicalColumn, line.Length)).Count(c => c == 't');
            return logicalColumn - tabCount + (tabCount * tabIndent);
        }
        public int GetVisualColumn(int logicalLine, int logicalColumn)
        {
            using (Graphics g = textArea.CreateGraphics())
            {
                return MeasureLogicalLineWidth(g, 0, logicalColumn, logicalLine) / WideSpaceWidth;
            }
        }
        public void Refresh()
        {
            if (lastFont != textArea.EditorOptions.Font.RegularFont)
            {
                OptionsChanged();
                textArea.Invalidate();
            }
        }
        Font lastFont;
        int fontHeight;
        public int FontHeight { get { return fontHeight; } }

        public void OptionsChanged()
        {
            this.lastFont = textArea.EditorOptions.Font.RegularFont;
            this.fontHeight = lastFont.GetFontHeight();
            this.SpaceWidth = Math.Max(GetWidth(' ', lastFont), 1);
            this.WideSpaceWidth = Math.Max(SpaceWidth, GetWidth('x', lastFont));
        }

        public int GetLogicalColumn(Graphics g, TextLine line, int start, int end, int drawingPos, int targetVisualPosX)
        {
            if (start == end)
                return -1;

            int tabIndent = textArea.EditorOptions.TabIndent;
            var font = textArea.EditorOptions.Font;

            var words = line.Words;
            if (words == null)
                return 0;
            int wordOffset = 0;
            foreach (var word in words)
            {
                if (wordOffset >= end)
                    return -1;
                if (wordOffset + word.Length >= start)
                {
                    int newDrawingPos = 0;
                    switch (word.Type)
                    {
                        case WordType.Space:
                            newDrawingPos = drawingPos + SpaceWidth;
                            if (newDrawingPos >= targetVisualPosX)
                                return IsNearerToAThanB(targetVisualPosX, drawingPos, newDrawingPos) ? wordOffset : wordOffset + 1;
                            break;
                        case WordType.Tab:
                            newDrawingPos = drawingPos + tabIndent * WideSpaceWidth;
                            if (newDrawingPos >= targetVisualPosX)
                                return IsNearerToAThanB(targetVisualPosX, drawingPos, newDrawingPos) ? wordOffset : wordOffset + 1;
                            break;
                        case WordType.Word:
                            var wordStart = Math.Max(wordOffset, start);
                            var wordLength = Math.Min(wordOffset + word.Length, end) - wordStart;
                            var text = textArea.Document.GetText(line.Offset + wordStart, wordLength);
                            var wordFont = font[word.SyntaxColor] ?? font.RegularFont;
                            newDrawingPos = drawingPos + MeasureStringWidth(g, text, wordFont);
                            if (newDrawingPos >= targetVisualPosX)
                            {
                                for (int j = 0; j < text.Length; j++)
                                {
                                    newDrawingPos = drawingPos + MeasureStringWidth(g, text[j].ToString(), wordFont);
                                    if (newDrawingPos >= targetVisualPosX)
                                    {
                                        if (IsNearerToAThanB(targetVisualPosX, drawingPos, newDrawingPos))
                                            return wordStart + j;
                                        else
                                            return wordStart + j + 1;
                                    }
                                }
                                return wordStart + text.Length;
                            }
                            break;
                        default:
                            break;
                    }
                }
                wordOffset += word.Length;
            }
            return wordOffset;
        }
        public int GetDrawingXPos(int logicalLine, int logicalColumn)
        {
            var foldings = document.FoldingManager.TopLevelFoldedFoldings.ToList();
            int i;
            var logicalLocation = new TextLocation(logicalColumn, logicalLine);
            FoldMarker f = null;

            for (i = foldings.Count - 1; i >= 0; --i)
            {
                if (foldings[i].Start < logicalLocation)
                {
                    f = foldings[i];
                    break;
                }
                if (foldings[i / 2].Start >= logicalLocation)
                    i /= 2;
            }
            int lastFolding = 0;
            int firstFolding = 0;
            int tabIndent = document.EditorOptions.TabIndent;
            int drawingPos;
            using (Graphics g = textArea.CreateGraphics())
            {
                // if no folding is interresting
                if (f == null)
                {
                    drawingPos = MeasureLogicalLineWidth(g, 0, logicalColumn, logicalLine);
                    return drawingPos - textArea.VisualLocation.X;
                }

                // if logicalLine/logicalColumn is in folding
                if (f.End.Line > logicalLine || f.End.Line == logicalLine && f.End.Column > logicalColumn)
                {
                    logicalColumn = f.Start.Column;
                    logicalLine = f.Start.Line;
                    --i;
                }
                lastFolding = i;

                // search backwards until a new visible line is reched
                for (; i >= 0; --i)
                {
                    f = (FoldMarker)foldings[i];
                    if (f.End.Line < logicalLine)
                    { // reached the begin of a new visible line
                        break;
                    }
                }
                firstFolding = i + 1;

                if (lastFolding < firstFolding)
                {
                    drawingPos = MeasureLogicalLineWidth(g, 0, logicalColumn, logicalLine);
                    return drawingPos - textArea.VisualLocation.X;
                }

                int foldEnd = 0;
                drawingPos = 0;
                for (i = firstFolding; i <= lastFolding; ++i)
                {
                    f = foldings[i];
                    drawingPos += MeasureLogicalLineWidth(g, foldEnd, f.Start.Column, f.Start.Line);
                    foldEnd = f.End.Column;
                    drawingPos += AdditionalFoldTextSize;
                    drawingPos += MeasureStringWidth(g, f.FoldText, document.EditorOptions.Font.RegularFont);
                }
                drawingPos += MeasureLogicalLineWidth(g, foldEnd, logicalColumn, logicalLine);
                return drawingPos - textArea.VisualLocation.X;
            }
        }
    }
}
