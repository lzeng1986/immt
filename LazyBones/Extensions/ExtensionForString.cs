namespace LazyBones.Extensions
{
    using System;
    using System.Linq;
    using System.Globalization;

    /// <summary>
    /// string类的扩展
    /// </summary>
    [Author("曾樑")]
    public static class ExtensionForString
    {
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 判断字符串是否有值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 判断字符串是否为空或者全部为空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrBlank(this string str)
        {
            return (str == null) || (str.Trim().Length == 0);
        }

        public static string Load(this string str, params object[] args)
        {
            return string.Format(str, args);
        }
        public static string FormatInvariant(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
        /// <summary>
        /// 对string类的SubString函数功能的扩展，主要扩展点在于起始位置startInd可以为任意int值
        /// 例如：-2代表字符串的倒数第2位的位置
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startInd">起始位置</param>
        /// <returns></returns>
        public static string Sub(this string str, int startInd)
        {
            if (startInd < 0 || startInd >= str.Length)
                startInd = startInd % str.Length;
            if (startInd < 0)
                startInd += str.Length;
            return str.Substring(startInd);
        }
        /// <summary>
        /// 对string类的SubString函数功能的扩展，主要扩展点在于起始位置startInd可以为任意int值
        /// 例如：-2代表字符串的倒数第2位的位置
        /// 如果len大小超出字符串长度，则自动截断至字符串长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startInd">起始位置</param>
        /// <param name="len">截取长度</param>
        /// <returns></returns>
        public static string Sub(this string str, int startInd, int len)
        {
            if (len < 0)
                throw new ArgumentOutOfRangeException("len", "len值不能小于0");
            if (startInd < 0 || startInd >= str.Length)
                startInd = startInd % str.Length;
            if (startInd < 0)
                startInd += str.Length;
            if (startInd + len > str.Length)
                return str.Substring(startInd);
            else
                return str.Substring(startInd, len);
        }
        /// <summary>
        /// 返回字符串中符合条件的部分
        /// </summary>
        /// <param name="str"></param>
        /// <param name="predicate">筛选条件</param>
        /// <returns></returns>
        public static string Sub(this string str, Func<char, bool> predicate)
        {
            var i = str.Where(predicate).ToArray();
            return new string(i);
        }

        /// <summary>
        /// 返回字符串头部长度为len的部分，如果len大小超出字符串长度，则自动截断至字符串长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len">截取长度</param>
        /// <returns></returns>
        public static string Head(this string str, int len)
        {
            if (len < 0)
                throw new ArgumentOutOfRangeException("len", "len值不能小于0");
            if (len > str.Length)
                len = str.Length;
            return str.Substring(0, len);
        }
        /// <summary>
        /// 返回字符串尾部长度为len的部分，如果len大小超出字符串长度，则自动截断至字符串长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len">截取长度</param>
        /// <returns></returns>
        public static string Tail(this string str, int len)
        {
            if (len < 0)
                throw new ArgumentOutOfRangeException("len", "len值不能小于0");
            if (len > str.Length)
                len = str.Length;
            var ind = str.Length - len;
            return str.Substring(ind);
        }
        public static bool IsInt16(this string str)
        {
            try
            {
                Int16 i;
                return Int16.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static Int16 ToInt16(this string str)
        {
            return Int16.Parse(str.Trim(), System.Globalization.NumberStyles.Any);
        }
        public static bool IsInt32(this string str)
        {
            try
            {
                int i;
                return Int32.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static int ToInt32(this string str)
        {
            return Int32.Parse(str.Trim(), System.Globalization.NumberStyles.Any);
        }

        public static bool IsDecimal(this string str)
        {
            try
            {
                decimal i;
                return decimal.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static decimal ToDecimal(this string str)
        {
            return decimal.Parse(str.Trim(), System.Globalization.NumberStyles.Any, null);
        }

        public static bool IsInt64(this string str)
        {
            try
            {
                Int64 i;
                return Int64.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static long ToInt64(this string str)
        {
            return Int64.Parse(str.Trim(), System.Globalization.NumberStyles.Any, null);
        }

        public static bool IsDouble(this string str)
        {
            try
            {
                double i;
                return double.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static double ToDouble(this string str)
        {
            return double.Parse(str.Trim(), System.Globalization.NumberStyles.Any, null);
        }

        public static bool IsFloat(this string str)
        {
            try
            {
                float i;
                return float.TryParse(str.Trim(), System.Globalization.NumberStyles.Any, null, out i);
            }
            catch { return false; }
        }
        public static float ToFloat(this string str)
        {
            return float.Parse(str.Trim(), System.Globalization.NumberStyles.Any, null);
        }

        public static bool IsDatetime(this string str)
        {
            try
            {
                DateTime i;
                return DateTime.TryParse(str.Trim(), out i);
            }
            catch { return false; }
        }
        public static DateTime ToDatetime(this string str)
        {
            return Convert.ToDateTime(str);
        }
        public static void ToDatetimeAndDo(this string str, Action<DateTime> convertSuccessAction)
        {
            try
            {
                var dt = DateTime.Parse(str.Trim());
                convertSuccessAction(dt);
            }
            catch { }
        }
        public static void ToDatetimeAndDo(this string str, Action<DateTime> convertSuccessAction, Action<Exception> convertFailedAction)
        {
            try
            {
                var dt = DateTime.Parse(str.Trim());
                convertSuccessAction(dt);
            }
            catch (Exception ex)
            {
                convertFailedAction(ex);
            }
        }
        public static bool IsBoolean(this string str)
        {
            try
            {
                bool i;
                return bool.TryParse(str.Trim(), out i);
            }
            catch { return false; }
        }
        public static bool ToBoolean(this string str)
        {
            return Convert.ToBoolean(str);
        }
        public static void ToBooleanAndDo(this string str, Action<bool> convertSuccessAction)
        {
            try
            {
                bool b = bool.Parse(str);
                convertSuccessAction(b);
            }
            catch { }
        }
        public static void ToBooleanAndDo(this string str, Action<bool> convertSuccessAction, Action<Exception> convertFailedAction)
        {
            try
            {
                bool b = bool.Parse(str);
                convertSuccessAction(b);
            }
            catch (Exception ex)
            {
                convertFailedAction(ex);
            }
        }
        public static bool IsGuid(this string str)
        {
            try
            {
                new Guid(str.Trim());
                return true;
            }
            catch { return false; }
        }
        public static Guid ToGuid(this string str)
        {
            return new Guid(str);
        }
        public static void ToGuidAndDo(this string str, Action<Guid> convertSuccessAction)
        {
            try
            {
                var guid = new Guid(str);
                convertSuccessAction(guid);
            }
            catch { }
        }
        public static void ToGuidAndDo(this string str, Action<Guid> convertSuccessAction, Action<Exception> convertFailedAction)
        {
            try
            {
                var guid = new Guid(str);
                convertSuccessAction(guid);
            }
            catch (Exception ex)
            {
                convertFailedAction(ex);
            }
        }
    }
}
