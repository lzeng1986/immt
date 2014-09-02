namespace LazyBones.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using LazyBones.Log;

    [Author("曾樑")]
    public static class ExtensionForType
    {
        public static void SafeDispose(this IDisposable obj)
        {
            if (obj != null)
                obj.Dispose();
        }
        public static void SetPropertyValue<T>(this T obj, string propertyName, object value)
        {
            var prop = System.ComponentModel.TypeDescriptor.GetProperties(obj)[propertyName];
            if (null == prop)
                throw new ArgumentException();
            prop.SetValue(obj, value);
        }
        public static T GetSingleAttribute<T>(this MemberInfo m)
            where T : Attribute
        {
            return m.GetSingleAttribute<T>(false);
        }
        public static T GetSingleAttribute<T>(this MemberInfo m, bool inherit)
            where T : Attribute
        {
            var attr = (T[])m.GetCustomAttributes(typeof(T), inherit);
            if (attr == null || attr.Length == 0)
                return null;
            return attr[0];
        }
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo m, bool b)
        {
            return m.GetCustomAttributes(typeof(T), b).OfType<T>();
        }
        public static Type[] GetTypeSafe(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                foreach (var ex in typeLoadException.LoaderExceptions)
                {
                    TinyLog.Warn("从程序集" + assembly + "加载类型出错:" + ex);
                }
                return typeLoadException.Types.Where(t => t != null).ToArray();
            }
        }
        public static object CreateInstance(this Type type, params object[] parameters)
        {
            var types = parameters == null ? Type.EmptyTypes : new Type[parameters.Length];
            var ctor = type.GetConstructor(types);
            if (ctor == null)
                throw new ArgumentException(string.Format("不存在签名为({0})的构造函数", string.Join(",", types.Select(t => t.FullName).ToArray())));
            return ctor.Invoke(parameters);
        }
        public static TType CreateInstance<TType>(this Type type, params object[] parameters)
        {
            return (TType)type.CreateInstance(parameters);
        }
        public static void SafeCall<TArgs>(this EventHandler<TArgs> handler, object sender, TArgs arg)
            where TArgs : EventArgs
        {
            var tmp = handler;
            if (tmp == null)
                return;
            foreach (var dl in tmp.GetInvocationList())
            {
                try
                {
                    dl.DynamicInvoke(sender, arg);
                }
                catch { }
            }
        }
        public static void SafeCall(this EventHandler handler, object sender)
        {
            var tmp = handler;
            if (tmp == null)
                return;
            foreach (var dl in tmp.GetInvocationList())
            {
                try
                {
                    dl.DynamicInvoke(sender, EventArgs.Empty);
                }
                catch { }
            }
        }
    }
}
