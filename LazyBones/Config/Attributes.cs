using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.Config
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DefaultAttribute : Attribute
    {
        /// <summary>
        /// 创建一个<see cref="DefaultAttribute"/>对象
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        public DefaultAttribute(object defaultValue)
        {
            Value = defaultValue;
        }
        /// <summary>
        /// 获取设置的默认值
        /// </summary>
        public object Value { get; private set; }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ItemAttribute : Attribute
    {
        public bool Required { get; set; }
        public object Default { get; set; }
        public string Name { get; private set; }
        public ItemAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            Name = name;
            Required = false;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DefaultItemAttribute : Attribute
    {
        public string Name { get; private set; }
        public DefaultItemAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ArrayItemAttribute : Attribute
    {
        public bool Required { get; set; }
        public object Default { get; set; }
        public string Name { get; private set; }
        public ArrayItemAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            Name = name;
            Required = false;
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ElementAttribute : Attribute
    {
        public string Name { get; private set; }
        public ElementAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 用于标记一个属性必须要赋值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class RequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// 添加此特性的类在应用程序整个生命周期里只会存在一个对象
    /// <para>这通常用来标记不需要添加额外配置的对象</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StaticAttribute : Attribute
    {
    }
}
