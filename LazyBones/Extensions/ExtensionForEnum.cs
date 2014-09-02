using System;

namespace LazyBones.Extensions
{
    public static class ExtensionForEnum
    {
        static void CheckEnumType<T>()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("类型" + typeof(T) + "不为枚举类型");
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
                throw new ArgumentException("类型" + typeof(T) + "必须标记FlagsAttribute");
        }
        public static bool HasFlag<T>(this T value, T flag) where T : struct
        {
            CheckEnumType<T>();
            var v = Convert.ToUInt64(value);
            var f = Convert.ToUInt64(flag);
            return (v & f) != 0;
        }
        public static T AddFlag<T>(this T value, T flag) where T : struct
        {
            CheckEnumType<T>();
            var v = Convert.ToUInt64(value);
            var f = Convert.ToUInt64(flag);
            return (T)Enum.ToObject(typeof(T), v | f);
        }
        public static T RemoveFlag<T>(this T value, T flag) where T : struct
        {
            CheckEnumType<T>();
            var v = Convert.ToUInt64(value);
            var f = Convert.ToUInt64(flag);
            return (T)Enum.ToObject(typeof(T), v & (~f));
        }
    }
}
