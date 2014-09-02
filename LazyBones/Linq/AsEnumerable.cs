using System.Collections.Generic;

namespace LazyBones.Linq
{
    public static partial class EnumerableMore
    {
        public static IEnumerable<T> AsEnumerable<T>(this T source)
        {
            yield return source;
        }
    }
}
