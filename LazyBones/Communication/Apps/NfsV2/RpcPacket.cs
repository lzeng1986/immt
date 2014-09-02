using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Apps.NfsV2
{
    public class RpcPacket : LBRawDataMessage
    {
        List<byte> data;
        private int length;
        private int offset = 0;
        public RpcPacket(byte[] data)
        {
            this.data = new List<byte>(data);
            length = data.Length;
        }
        public RpcPacket(byte[] data,int offset,int count)
        {
            this.data = new List<byte>(data.Skip(offset).Take(count));
            length = data.Length;
        }
        public override byte[] RawData
        {
            get { return data.ToArray(); }
            set
            {
                this.data.Clear();
                this.data.AddRange(data);
            }
        }
        public int Length
        {
            get { return data.Count; }
        }
        public void Jump(int len)
        {
            CheckOffset(len);
            offset += len;
        }
        void CheckOffset(int lookAhead)
        {
            if (offset + lookAhead > length)
                throw new IndexOutOfRangeException();
        }
        public int GetInt32()
        {
            return (int)GetUInt32();
        }
        public uint GetUInt32()
        {
            CheckOffset(4);
            uint one = data[offset];
            one <<= 24;
            uint two = data[offset + 1];
            two <<= 16;
            uint three = data[offset + 2];
            three <<= 8;
            uint four = data[offset + 3];
            var result = one | two | three | four;
            offset += 4;
            return result;
        }
        public string GetString()
        {
            var len = GetInt32();
            CheckOffset(len);
            var buf = new char[len];
            for (var i = 0; i < len; i++)
            {
                buf[i] = (char)data[offset + i];
            }
            offset += len;
            if (len % 4 != 0)
                offset += (4 - offset % 4);
            return new string(buf);
        }
        public char GetChar()
        {
            CheckOffset(1);
            var value = data[offset];
            offset++;
            return (char)value;
        }
        public byte[] GetData()
        {
            var len = (int)GetInt32();
            CheckOffset(len);
            var buf = new Byte[len];
            data.CopyTo(offset, buf, 0, len);
            offset += len;
            if (len % 4 != 0)
                offset += (4 - offset % 4);
            return buf;
        }
        public DateTime GetDateTime()
        {
            var seconds = GetUInt32();
            var milliSeconds = GetUInt32();
            var ticks = (long)seconds * 1000 * 1000 * 10 + milliSeconds * 10;
            return DateTime.FromFileTimeUtc(ticks);
        }
        public void Append(int value)
        {
            Append((uint)value);
        }
        public void Append(uint value)
        {
            var one = (byte)value;
            var two = (byte)(value >>= 8);
            var three = (byte)(value >>= 8);
            var four = (byte)(value >>= 8);
            data.Add(four);
            data.Add(three);
            data.Add(two);
            data.Add(one);
            offset += 4;
        }
        public void Append(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Append(0);
            }
            else
            {
                Append(value.Length);
                for (var i = 0; i < value.Length; i++)
                {
                    data.Add((byte)value[i]);
                }
                while (data.Count % 4 != 0) data.Add(0);
            }
        }
        public void Append(byte[] value)
        {
            if (value == null)
            {
                Append(0);
            }
            else
            {
                Append(value.Length);
                data.AddRange(value);
                while (data.Count % 4 != 0) data.Add(0);
            }
        }
        public void Append(byte[] value, int len)
        {
            if (value == null)
            {
                Append(0);
            }
            else
            {
                Append(value.Length);
                data.AddRange(value.Take(len));
                while (data.Count % 4 != 0) data.Add(0);
            }
        }
        public void Append(byte[] value, int offset, int len)
        {
            if (value == null)
            {
                Append(0);
            }
            else
            {
                Append(value.Length);
                data.AddRange(value.Skip(offset).Take(len));
                while (data.Count % 4 != 0) data.Add(0);
            }
        }
        /// rfc1094[1989.3]-[2.3.4]-[page 15]
        /// rfc1094中的 timeval 结构，用于表示Nfs系统时间
        public void Append(DateTime dateTime)
        {
            var ticks = dateTime.ToFileTimeUtc();
            //msdn:以 100 毫微秒(注：毫微秒即纳秒)为间隔的间隔数,将间隔数转换成秒数
            //实际上就是以0.1毫秒为单位
            var seconds = unchecked((uint)(ticks / 10000000));
            var milliSeconds = (uint)((ticks % 10000) / 10);
            Append(seconds);
            Append(milliSeconds);
        }
    }
}
