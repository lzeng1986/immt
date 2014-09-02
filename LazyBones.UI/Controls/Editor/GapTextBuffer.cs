using System;
using System.Text;

namespace LazyBones.UI.Controls.Editor
{
    //实现一个间隙缓冲区，具有很高的插入、删除性能，此缓冲区不保证写入时的线程安全
    class GapTextBuffer
    {
        const int GapMinSize = 128;
        const int GapMaxSize = 1024;
        char[] buffer = new char[0];
        int gapStart = 0;
        int gapEnd = 0;
        int gapSize = 0; //gapsize = gapEndOffset - gapStartOffset;

        public int Length
        {
            get { return buffer.Length - gapSize; }
        }
        public string Text
        {
            get { return GetText(0, Length); }
            set
            {
                buffer = value == null ? string.Empty.ToCharArray() : value.ToCharArray();
                gapStart = gapEnd = gapSize = 0;
            }
        }
        public char this[int ind]
        {
            get
            {
                if (ind < 0 || Length <= ind)
                    throw new ArgumentOutOfRangeException("ind", ind, "应在0和" + Length + "之间");
                return ind < gapStart ? buffer[ind] : buffer[ind + gapSize];
            }
            set
            {
                if (ind < 0 || Length <= ind)
                    throw new ArgumentOutOfRangeException("ind", ind, "应在0和" + Length + "之间");
                if (ind < gapStart)
                    buffer[ind] = value;
                else
                    buffer[ind + gapSize] = value;
            }
        }
        public string GetText(int offset, int length)
        {
            if (offset < 0 || Length < offset)
                throw new ArgumentOutOfRangeException("offset", offset, "应在0和" + Length + "之间");
            if (length < 0 || Length < offset + length)
                throw new ArgumentOutOfRangeException("length", length, "应在0和" + Length + "之间");

            var end = offset + length;
            if (end < gapStart)
                return new string(buffer, offset, length);
            if (offset > gapEnd)
                return new string(buffer, offset + gapSize, length);

            var len1 = gapStart - offset;
            var len2 = end - gapStart;
            var sb = new StringBuilder(length);
            sb.Append(buffer, offset, len1);
            sb.Append(buffer, gapEnd, len2);
            return sb.ToString();
        }
        public void Insert(int offset, string text)
        {
            Replace(offset, 0, text);
        }
        public void Remove(int offset, int length)
        {
            Replace(offset, length, string.Empty);
        }
        public void Replace(int offset, int length, string text)
        {
            if (text == null)
                text = string.Empty;

            if (offset < 0 || Length <= offset)
                throw new ArgumentOutOfRangeException("offset", offset, "应在0和" + Length + "之间");
            if (length < 0 || Length < offset + length)
                throw new ArgumentOutOfRangeException("length", length, "应在0和" + Length + "之间");

            MoveGap(offset, text.Length - length);
            gapEnd += length;//删除字符
            text.CopyTo(0, buffer, gapStart, text.Length);
            gapStart += text.Length;
            gapSize = gapEnd - gapStart;
            if (gapSize > GapMaxSize)
            {
                RebuildBuffer(gapStart, GapMinSize);
            }
        }
        //移动间隙位置
        void MoveGap(int newOffset, int minRequiredSize)
        {
            if (minRequiredSize > gapSize)
            {
                //当前间隙不足时，重新分配缓冲区，使间隙达到minRequiredSize
                RebuildBuffer(newOffset, minRequiredSize);
            } 
            else
            {
                while (gapStart > newOffset)
                {
                    buffer[gapEnd--] = buffer[gapStart--];
                }
                while (gapStart < newOffset)
                {
                    buffer[gapStart++] = buffer[gapEnd++];
                }
            }
        }
        void RebuildBuffer(int newGapOffset, int newGapSize)
        {
            newGapSize = Math.Max(newGapSize, GapMinSize);
            var newBuffer = new char[Length + newGapSize];
            //分三部分将旧缓冲区内的数据复制到新缓冲区
            if (newGapOffset < gapStart)
            {                
                Buffer.BlockCopy(buffer, 0, newBuffer, 0, newGapOffset);
                Buffer.BlockCopy(buffer, newGapOffset, newBuffer, newGapOffset + newGapSize, gapStart - newGapOffset);
                Buffer.BlockCopy(buffer, gapEnd, newBuffer, newGapSize + gapStart, buffer.Length - gapEnd);
            } 
            else
            {
                Buffer.BlockCopy(buffer, 0, newBuffer, 0, gapStart);
                Buffer.BlockCopy(buffer, gapEnd, newBuffer, gapStart, newGapOffset - gapStart);
                Buffer.BlockCopy(buffer, newGapOffset + gapSize, newBuffer, newGapOffset + newGapSize, newBuffer.Length - newGapOffset - newGapSize);
            }
            gapStart = newGapOffset;
            gapSize = newGapSize;
            gapEnd = gapStart + gapSize;
            buffer = null;
            buffer = newBuffer;
        }
    }
}
