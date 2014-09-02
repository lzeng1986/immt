using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using LazyBones.Config;
using LazyBones.Log.Config;
using LazyBones.Log.Layouts;

namespace LazyBones.Log.Core
{
    internal static class Helper
    {
        /// <summary>
        /// 检查<see cref="Exception"/>是否应该被抛出
        /// </summary>
        /// <param name="ex"></param>
        public static void Filter(this Exception ex)
        {
            if (ex is StackOverflowException)
            {
                throw ex;
            }
            if (ex is ThreadAbortException)
            {
                throw ex;
            }
            if (ex is OutOfMemoryException)
            {
                throw ex;
            }
            if (ex is LazyBones.Config.ConfigException)
            {
                throw ex;
            }
        }

        public static object CreateInstance(Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
                return ctor.Invoke(null);
            else
                throw new LogConfigException("");
        }
        public static T ConvertFromString<T>(string valueString, T defaultValue)
        {
            object result;
            var convertType = typeof(T);
            if (!TryImplicitConvert(convertType, valueString, out result))
            {
                if (typeof(T).IsEnum)
                {
                    result = Enum.Parse(convertType, valueString, true);
                }
                else
                {
                    result = Convert.ChangeType(valueString, convertType);
                }
            }
            if (result == null)
                return defaultValue;
            return (T)result;
        }
        public static T ConvertFromString<T>(string valueString, LogConfigItemFactory itemFactory)
        {
            return (T)ConvertFromString(typeof(T), valueString, itemFactory);
        }
        public static object ConvertFromString(Type convertType, string valueString, LogConfigItemFactory itemFactory)
        {
            object result = null;
            if (!TrySpecificConvert(convertType, valueString, itemFactory, out result))
            {
                if (!TryImplicitConvert(convertType, valueString, out result))
                {
                    if (convertType.IsEnum)
                    {
                        result = Enum.Parse(convertType, valueString, true);
                    }
                    else
                    {
                        result = Convert.ChangeType(valueString, convertType);
                    }
                }
            }
            if (result != null)
                return result;
            throw new InvalidCastException("无法进行指定类型转换");
        }
        static IEnumerable<PropertyInfo> GetAllReadableProperties(object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
        }
        static IEnumerable<PropertyInfo> GetAllReadableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);
        }
        //检查标记RequiredAttribute的属性是否被赋值，如果没有，则抛出异常
        public static void CheckRequired(object obj)
        {
            foreach (var pi in GetAllReadableProperties(obj))
            {
                if (pi.IsDefined(typeof(RequiredAttribute), false))
                {
                    var v = pi.GetValue(obj, null);
                    if (v == null)
                    {
                        throw new LogConfigException("{0}对象的必要参数{1}未赋值", obj, pi.Name);
                    }
                }
            }
        }
        /// <summary>
        /// 对所有标记了DefaultValueAttribute特性的属性根据DefaultValueAttribute为其赋值
        /// </summary>
        /// <param name="obj">需要赋值的对象</param>
        public static void ResetPropertyValue(object obj, LogConfigItemFactory itemFactory)
        {
            foreach (var pi in GetAllReadableProperties(obj))
            {
                var attrs = (DefaultValueAttribute[])pi.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    object result;
                    if (attrs[0].Value is string)
                    {
                        result = Helper.ConvertFromString(pi.PropertyType, (string)attrs[0].Value, itemFactory);
                    }
                    else
                    {
                        if (!TryImplicitConvert(pi.PropertyType, attrs[0].Value, out result))
                        {
                            result = Convert.ChangeType(attrs[0].Value, pi.PropertyType);
                        }
                    }
                    if (result != null)
                    {
                        pi.SetValue(obj, result, null);
                    }
                    else
                    {
                        
                        TinyLog.Error("属性赋值失败：{0}.{1}", obj.GetType(), pi.Name);
                    }
                }
            }
        }

        public static PropertyInfo GetPropertyInfo(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        public static PropertyInfo GetPropertyInfoWithThrow(object obj, string propertyName)
        {
            var pi = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (pi != null)
            {
                return pi;
            }
            throw new ArgumentException("在对象" + obj + "中未找到名为" + propertyName + "的属性");
        }
        //尝试使用implicit运算符进行转换
        static bool TryImplicitConvert(Type convertType, object value, out object result)
        {
            var implicitMethod = convertType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] { value.GetType() }, null);
            if (implicitMethod == null)
            {
                result = null;
                return false;
            }
            result = implicitMethod.Invoke(null, new object[] { value });
            return true;
        }
        static bool TrySpecificConvert(Type convertType, string value, LogConfigItemFactory itemFactory, out object result)
        {
            result = null;
            if (typeof(LayoutBase).IsAssignableFrom(convertType))
            {
                result = new DefaultLayout(value, itemFactory);
                return true;
            }
            if (convertType == typeof(LogLevel))
            {
                result = (LogLevel)value;
                return true;
            }
            if (convertType == typeof(Encoding))
            {
                result = Encoding.GetEncoding(value);
                return true;
            }
            if (convertType == typeof(Type))
            {
                result = Type.GetType(value, false);
                return result != null;
            }
            if (convertType == typeof(Uri))
            {
                result = new Uri(value, UriKind.RelativeOrAbsolute);
                return true;
            }
            if (convertType == typeof(CultureInfo))
            {
                result = new CultureInfo(value);
                return true;
            }
            return false;
        }
        //根据属性名称和字符串形式的属性值对属性进行赋值
        public static void SetPropertyFromString(object obj, string propertyName, string propertyValue, LogConfigItemFactory itemFactory)
        {
            try
            {
                var pi = GetPropertyInfoWithThrow(obj, propertyName);
                if (pi.PropertyType.IsArray)
                {
                    throw new NotSupportedException("对象" + obj + "的属性" + propertyName + "是数组类型！本系统不支持从配置文件对数组类型的属性进行赋值");
                }
                object value = Helper.ConvertFromString(pi.PropertyType, propertyValue, itemFactory);
                pi.SetValue(obj, value, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new LogConfigException(ex.InnerException,"在对<{0}>的属性<{1}>赋值时出现错误：{2}", obj, propertyName, ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                throw new LogConfigException(ex,"在对<{0}>的属性<{1}>赋值时出现错误：{2}", obj, propertyName, ex.Message);
            }
        }
        /// <summary>
        /// 获取继承或实现了<typeparam name="T"/>的对象
        /// </summary>
        /// <typeparam name="T">基类</typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static T[] GetObjects<T>(params object[] objects)
            where T : class
        {
            if (objects == null)
                return new T[0];
            var result = new List<T>();
            var visitedObjs = new HashSet<object>();
            foreach (var obj in objects)
            {
                ScanProperties(result, obj, visitedObjs);
            }
            visitedObjs.Clear();
            return result.ToArray();
        }
        static void ScanProperties<T>(List<T> result, object obj, HashSet<object> visitedObjs)
        {
            if (obj == null || !obj.GetType().IsDefined(typeof(ConfigItemAttribute), true))
                return;

            if (visitedObjs.Contains(obj))
                return;

            visitedObjs.Add(obj);

            if (obj is T)
                result.Add((T)obj);

            foreach (var p in GetAllReadableProperties(obj))
            {
                if (p.PropertyType.IsPrimitive || p.PropertyType.IsEnum || p.PropertyType == typeof(string))
                    continue;
                var value = p.GetValue(obj, null);
                if (value is IEnumerable)
                {
                    foreach (var v in (value as IEnumerable))
                    {
                        ScanProperties(result, v, visitedObjs);
                    }
                }
                else
                {
                    ScanProperties(result, value, visitedObjs);
                }
            }
        }
    }
}
