using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using LazyBones.Extensions;

namespace LazyBones.Config
{
    public static class ConfigConvert
    {
        public static XElement Value2Xml(object value)
        {
            var type = value.GetType();
            var elementAttribute = type.GetSingleAttribute<ElementAttribute>();
            if (elementAttribute == null)
                throw new ArgumentNullException(type + "缺少ElementAttribute");
            return new XElement(elementAttribute.Name, GetContent(type, value).ToArray());
        }
        static IEnumerable<object> GetContent(Type type, object obj)
        {
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
            {
                var itemAttribute = property.GetSingleAttribute<ItemAttribute>();
                if (itemAttribute == null)
                    continue;
                var propertyType = property.PropertyType;
                var value = property.GetValue(obj, null);
                if (propertyType.IsArray)
                {
                    var array = value as Array;
                    if (array != null && array.Length > 0)
                    {
                        //yield return new XElement(itemAttribute.Name, array.Cast<object>().Select(Value2Xml).ToArray());
                    }
                }
                else if (propertyType.IsDefined(typeof(ElementAttribute), false))
                {
                    yield return Value2Xml(value);
                }
                else
                {
                    if (itemAttribute.Required || value != itemAttribute.Default)
                    {
                        yield return new XAttribute(itemAttribute.Name, value);
                    }
                }
            }
        }
        struct Prop
        {
            public ItemAttribute ItemAttribute;
            public PropertyInfo Property;
        }
        public static object Xml2Value(Type objType, XElement source)
        {
            var ctor = objType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                throw new ConfigException(objType + "缺少空参数构造函数");
            return Xml2ValueInternal(objType, source, ctor.Invoke(Type.EmptyTypes));
        }
        public static T Xml2Value<T>(XElement source)
            where T : new()
        {
            return (T)Xml2ValueInternal(typeof(T), source, new T());
        }
        static object Xml2ValueInternal(Type objType, XElement source, object obj)
        {
            if (GetElementAttributeName(objType) != source.Name.LocalName)
                throw new ConfigException(objType + "ElementAttribute.Name与配置项名称不一致");
            var properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);
            foreach (var p in GetProperties(properties))
            {
                object value;
                if (p.Property.PropertyType.IsArray)
                {
                    value = MakeArrayValue(source.Element(p.ItemAttribute.Name), p.ItemAttribute, p.Property);
                }
                else
                {
                    var attr = source.Attribute(p.ItemAttribute.Name);
                    if (attr == null && p.ItemAttribute.Required)
                    {
                        throw new ConfigException("缺少必要属性" + p.ItemAttribute.Name);
                    }
                    if (attr == null || !TryConvertFromString(p.Property.PropertyType, attr.Value, out value))
                    {
                        value = p.ItemAttribute.Default;
                    }
                }
                p.Property.SetValue(obj, value, null);
            }
            return obj;
        }
        static Array MakeArrayValue(XElement source, ItemAttribute itemAttribute, PropertyInfo property)
        {
            if (source == null && itemAttribute.Required)
                throw new ConfigException("缺少必要属性" + itemAttribute.Name);
            if (!property.PropertyType.IsArray)
                throw new ConfigException("属性不为数组类型");

            var ctor = property.PropertyType.GetConstructor(new[] { typeof(int) });
            if (source == null)
                return (Array)ctor.Invoke(new object[] { 0 });

            var arrayElementType = property.PropertyType.GetElementType();
            var elementName = GetElementAttributeName(arrayElementType);
            var es = source.Elements(elementName).ToArray();
            var array = (Array)ctor.Invoke(new object[] { es.Length });
            for (var i = 0; i < es.Length; i++)
                array.SetValue(Xml2Value(property.PropertyType, es[i]), i);
            return array;
        }
        static IEnumerable<Prop> GetProperties(IEnumerable<PropertyInfo> props)
        {
            foreach (var p in props)
            {
                var itemAttribute = p.GetSingleAttribute<ItemAttribute>();
                if (itemAttribute == null)
                    continue;
                yield return new Prop { ItemAttribute = itemAttribute, Property = p };
            }
        }
        static string GetElementAttributeName(Type type)
        {
            var e = type.GetSingleAttribute<ElementAttribute>();
            if (e == null)
                throw new ConfigException(type + "缺少ElementAttribute");
            return e.Name;
        }
        public static T ConvertFromString<T>(string value)
        {
            object obj;
            if (TryConvertFromString(typeof(T), value, out obj))
                return (T)obj;
            else
                throw new InvalidCastException("无法完成相应转换");
        }
        public static T ConvertFromString<T>(string value, T defaultValue)
        {
            object obj;
            if (TryConvertFromString(typeof(T), value, out obj))
                return (T)obj;
            else
                return defaultValue;
        }
        public static bool TryConvertFromString<T>(string value, out T result)
        {
            object tmp;
            if (TryConvertFromString(typeof(T), value, out tmp))
            {
                result = (T)tmp;
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }
        public static bool TryConvertFromString(Type convertType, string value, out object result)
        {
            if (TrySpecificStringConvert(convertType, value, out result))
                return true;
            if (TryImplicitStringConvert(convertType, value, out result))
                return true;
            try
            {
                if (convertType.IsEnum)
                {
                    result = Enum.Parse(convertType, value, true);
                }
                else
                {
                    result = Convert.ChangeType(value, convertType);
                }
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        static bool TrySpecificStringConvert(Type convertType, string value, out object result)
        {
            result = null;
            if (convertType == typeof(Encoding))
            {
                result = Encoding.GetEncoding(value);
                return true;
            }
            if (convertType == typeof(Type))
            {
                result = Type.GetType(value);
                return true;
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
        static bool TryImplicitStringConvert(Type convertType, string value, out object result)
        {
            var implicitMethod = convertType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (implicitMethod == null)
            {
                result = null;
                return false;
            }
            result = implicitMethod.Invoke(null, new object[] { value });
            return true;
        }
    }
}
