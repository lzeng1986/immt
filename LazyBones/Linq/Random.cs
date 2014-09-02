using System;
using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        //随机数生成器
        public static IEnumerable<int> Randoms()
        {
            return Randoms(int.MaxValue);
        }
        public static IEnumerable<int> Randoms(int maxValue)
        {
            var rnd = new Random();
            while (true)
            {
                yield return rnd.Next(maxValue);
            }
        }
        public static IEnumerable<int> Randoms(int minValue, int maxValue)
        {
            var rnd = new Random();
            while (true)
            {
                yield return rnd.Next(minValue, maxValue);
            }
        }
        public static IEnumerable<double> RandomDoubles()
        {
            var rnd = new Random();
            while (true)
            {
                yield return rnd.NextDouble();
            }
        }
        public static IEnumerable<byte> RandomBytes()
        {
            var rnd = new Random();
            var buffer = new byte[100];
            while (true)
            {
                rnd.NextBytes(buffer);
                foreach (var b in buffer)
                    yield return b;
            }
        }
    }
}
