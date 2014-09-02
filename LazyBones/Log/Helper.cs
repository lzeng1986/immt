using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using LazyBones.Config;
using LazyBones.Log.Config;
using LazyBones.Log.Layouts;

namespace LazyBones.Log
{
    internal static class Helper
    {
        /// <summary>
        /// 检查<see cref="Exception"/>是否应该被抛出
        /// </summary>
        /// <param name="ex"></param>
        public static void Check(this Exception ex)
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

        static object ConvertFromString(Type convertType, string valueString, LogConfigItemFactory itemFactory)
        {
            object result = null;
            if (!ConfigConvert.TryConvertFromString(convertType, valueString, out result))
            {
                if (typeof(Layout).IsAssignableFrom(convertType))
                {
                    result = Layout.Create(valueString, itemFactory);
                }
                else if (convertType == typeof(LogLevel))
                {
                    result = (LogLevel)valueString;
                }
                else
                    throw new InvalidCastException("无法进行指定类型转换");
            }
            return result;
        }

        /// <summary>
        /// 对所有标记了DefaultValueAttribute特性的属性根据DefaultValueAttribute为其赋值
        /// </summary>
        /// <param name="obj">需要赋值的对象</param>
        public static void ResetPropertyValue(object obj, LogConfigItemFactory itemFactory)
        {
            foreach (PropertyDescriptor pi in TypeDescriptor.GetProperties(obj))
            {
                var defaultAttr = (DefaultValueAttribute)pi.Attributes[typeof(DefaultValueAttribute)];
                if (defaultAttr != null)
                {
                    try
                    {
                        pi.ResetValue(obj);
                    }
                    catch
                    {
                        object result = null;
                        if (defaultAttr.Value != null && pi.PropertyType.IsAssignableFrom(defaultAttr.Value.GetType()))
                        {
                            result = defaultAttr.Value;
                        }
                        else if (defaultAttr.Value is string)
                        {
                            result = Helper.ConvertFromString(pi.PropertyType, (string)defaultAttr.Value, itemFactory);
                        }
                        if (result != null)
                        {
                            pi.SetValue(obj, result);
                        }
                        else
                        {
                            TinyLog.Error("属性赋值失败：{0}.{1}", obj.GetType().Name, pi.Name);
                        }
                    }
                }
            }
        }
        public static void Validate(object obj)
        {
            foreach (PropertyDescriptor pi in TypeDescriptor.GetProperties(obj))
            {
                var value = pi.GetValue(obj);
                var required = (RequiredAttribute)pi.Attributes[typeof(RequiredAttribute)];
                if (required != null && value == null)
                {
                    throw new LogConfigException("{0}.{1}标记RequiredAttribute，但没有赋值", obj.GetType().Name, pi.Name);
                }

                foreach (var validator in pi.Attributes.OfType<ValidatorAttribute>())
                {
                    if (!validator.Validate(value))
                        throw new LogConfigException("{0}.{1}没有通过{2}的验证", obj.GetType().Name, pi.Name, validator.GetType().Name);
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
        //根据属性名称和字符串形式的属性值对属性进行赋值
        public static void SetPropertyFromString(object obj, string propertyName, string propertyValue, LogConfigItemFactory itemFactory)
        {
            try
            {
                var pi = GetPropertyInfoWithThrow(obj, propertyName);
                if (pi.PropertyType.IsArray)
                {
                    throw new NotSupportedException(obj.GetType().Name + '.' + propertyName + "是数组类型！本系统不支持从配置文件对数组类型的属性进行赋值");
                }
                object value = Helper.ConvertFromString(pi.PropertyType, propertyValue, itemFactory);
                pi.SetValue(obj, value, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new LogConfigException(ex.InnerException, "在对<{0}>的属性<{1}>赋值时出现错误：{2}", obj, propertyName, ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                throw new LogConfigException(ex, "在对<{0}>的属性<{1}>赋值时出现错误：{2}", obj, propertyName, ex.Message);
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

            foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(obj))
            {
                if (p.PropertyType.IsPrimitive || p.PropertyType.IsEnum || p.PropertyType == typeof(string))
                    continue;
                var value = p.GetValue(obj);
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
        public static bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            var fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                fileLength = fi.Length;
                lastWriteTime = fi.LastWriteTime;
                return true;
            }
            else
            {
                fileLength = -1;
                lastWriteTime = DateTime.MinValue;
                return false;
            }
        }
    }
}
