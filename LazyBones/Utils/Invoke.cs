using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LazyBones.Utils
{
    public static class Invoke
    {
        static BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        public static Function InvokeAction(this object instance, string methodName, params object[] args)
        {
            var member = instance.GetType().GetMember(methodName, defaultFlags);
            return null;
        }
    }
    public delegate object Function(params object[] args);
}
