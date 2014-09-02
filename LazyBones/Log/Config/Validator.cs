using System;
using System.Collections.Generic;
using System.Reflection;
using LazyBones.Config;

namespace LazyBones.Log.Config
{
    internal static class Validator
    {
        public static void Check(object obj)
        {

        }

        public static void CheckRequired(object obj)
        {
            foreach (var p in GetValidProperties(obj.GetType()))
            {
                if (p.IsDefined(typeof(RequiredAttribute), false))
                {
                    var value = p.GetValue(obj, null);
                    if (value == null)
                    {
                        throw new LogConfigException(string.Format("<{0}>.<{1}> 被标记为RequiredAttribute，但没有指定值",obj,p.Name));
                    }
                }
            }
        }

        static IEnumerable<PropertyInfo> GetValidProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
    }
}
