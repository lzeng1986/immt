using System.Collections.Generic;
using System.Linq;
using System;
using LazyBones.Collection;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 表示编辑器中的一行,包含除文本内容的所有信息
    /// </summary>
    public class TextLine : ISegment
    {
        List<TextWord> words = new List<TextWord>();
        WeakCollection<TextAnchor> anchors;
        /// <summary>
        /// 行号
        /// </summary>
        internal int LineNo { get; set; }
        /// <summary>
        /// 行起始位置偏移
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// 此行包含字符串长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 此行之前所有字符串总长度，包含此行字符串
        /// </summary>
        internal int TotalLength;
        public int DelimiterLength { get; internal set; }
        internal List<TextWord> Words
        {
            get { return words; }
        }

        internal RBNode<TextLine> Node;

        internal TextWord GetWord(int column)
        {
            return words.FirstOrDefault(w => w.Offset <= column && w.Length + w.Offset > column);
        }
        internal TextDrawInfo GetDrawInfo(int column)
        {
            var c = words.FirstOrDefault(w => w.Offset <= column && w.Length + w.Offset > column);
            return c == null ? TextDrawInfo.Default : c.SyntaxColor;
        }
        public TextAnchor CreateAnchor(int column)
        {
            if (column < 0 || Length < column)
                throw new ArgumentOutOfRangeException("column");
            var anchor = new TextAnchor(this, column);
            AddAnchor(anchor);
            return anchor;
        }

        void AddAnchor(TextAnchor anchor)
        {
            if (anchors == null)
                anchors = new WeakCollection<TextAnchor>();
            anchors.Add(anchor);
        }
        internal void Delete(LazyDeleter deleter)
        {
            if (anchors != null)
            {
                foreach (var a in anchors)
                    a.Delete(deleter);
                anchors = null;
            }
        }
        /// <summary>
        /// 移除行一部分数据
        /// </summary>
        internal void Remove(LazyDeleter deleter, int startColumn, int length)
        {
            if (length <= 0)
                return;
            if (anchors == null)
                return;
            var deletedAnchors = new List<TextAnchor>();
            foreach (var a in anchors.Where(a=>a.ColumnNo > startColumn))
            {
                if (a.ColumnNo >= startColumn + length)
                {
                    a.ColumnNo -= length;
                }
                else
                {
                    a.Delete(deleter);
                    deletedAnchors.Add(a);
                }
            }
            deletedAnchors.ForEach(a => anchors.Remove(a));
            deletedAnchors = null;
        }

        internal void Insert(int startColumn, int length)
        {
            if (length <= 0)
                return;
            if (anchors == null)
                return;
            foreach (var a in anchors)
            {
                if ((a.Type == AnchorType.BeforeInsert && a.ColumnNo > startColumn) || (a.Type == AnchorType.AfterInsert && a.ColumnNo >= startColumn))
                {
                    a.ColumnNo += length;
                }
            }
        }
        internal void MergedWith(TextLine deletedLine, int firstLineLength)
        {
            if (deletedLine.anchors != null)
            {
                foreach (var a in deletedLine.anchors)
                {
                    a.Line = this;
                    AddAnchor(a);
                    a.ColumnNo += firstLineLength;
                }
                deletedLine.anchors = null;
            }
        }

        /// <summary>
        /// Is called after a newline was inserted into this line, splitting it into this and followingLine.
        /// </summary>
        internal void SplitTo(TextLine followingLine)
        {
            if (anchors != null)
            {
                var movedAnchors = new List<TextAnchor>();
                foreach (TextAnchor a in anchors)
                {
                    if (a.Type == AnchorType.BeforeInsert
                        ? a.ColumnNo > this.Length
                        : a.ColumnNo >= this.Length)
                    {
                        a.Line = followingLine;
                        followingLine.AddAnchor(a);
                        a.ColumnNo -= this.Length;

                        movedAnchors.Add(a);
                    }
                }
                foreach (TextAnchor a in movedAnchors)
                {
                    anchors.Remove(a);
                }
            }
        }
    }
}
