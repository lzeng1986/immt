using System;
using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        /// <summary>
        /// 整数生成器
        /// </summary>
        public static IEnumerable<int> Generator()
        {
            return Generator(0, 1);
        }
        public static IEnumerable<int> Generator(int from)
        {
            return Generator(from, 1);
        }
        public static IEnumerable<int> Generator(int from, int step)
        {
            if (step <= 0)
                throw new ArgumentOutOfRangeException("step", "step不得小于或等于0");
            int i = from;
            while (true)
            {
                yield return i;
                i += step;
            }
        }
    }
}
