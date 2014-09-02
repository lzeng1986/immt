using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LazyBones.Config
{
    /// <summary>
    /// 用于读取Xml配置文件中的节
    /// </summary>
    public class ConfigElement
    {
        readonly XElement element;
        readonly string elementName;
        readonly Dictionary<string, string> attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ConfigElement[] children;
        /// <summary>
        /// 从<see cref="XElement"/>对象创建一个<see cref="ConfigElement"/>对象
        /// </summary>
        /// <param name="source">数据源</param>
        public ConfigElement(XElement source)
        {
            element = source;
            elementName = element.Name.LocalName;
            Parser();
        }
        /// <summary>
        /// 检查当前元素节点名称是否满足要求，检查过程忽略字母大小写
        /// </summary>
        /// <param name="expectNames">期望的元素名称</param>
        public void CheckName(params string[] expectNames)
        {
            if (expectNames == null || Array.Find(expectNames, n => n == elementName) != null)
                return;
            throw new ConfigException("解析配置文件失败，节点名称应为：" + elementName + "，而此处为：" + string.Join("|", expectNames));
        }
        /// <summary>
        /// 获取该节点的名称
        /// </summary>
        public string Name
        {
            get { return elementName; }
        }
        /// <summary>
        /// 获取节点所包含的Attribute
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return attributes; }
        }
        /// <summary>
        /// 获取节点所有子节点
        /// </summary>
        public ConfigElement[] Children
        {
            get { return children; }
        }
        /// <summary>
        /// 获取指定名称的子节点，忽略名称大小写
        /// </summary>
        /// <param name="name">子节点名称</param>
        /// <returns>获取的子节点</returns>
        public IEnumerable<ConfigElement> Elements(string name)
        {
            return children.Where(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
        /// <summary>
        /// 获取指定名称的Attribute值，名称忽略大小写
        /// </summary>
        /// <param name="attributeName">Attribute名称</param>
        /// <returns>Attribute值</returns>
        public string GetRequiredAttribute(string attributeName)
        {
            if (attributes.ContainsKey(attributeName))
            {
                return attributes[attributeName];
            }
            throw new ConfigException("配置文件节点{0}缺少必须项{1}", elementName, attributeName);
        }
        /// <summary>
        /// 获取可选属性值，如不存在或者不是指定类型，则返回默认值
        /// </summary>
        /// <param name="attributeName">Attribute名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>获取的值</returns>
        public T GetOptionalAttribute<T>(string attributeName, T defaultValue)
        {
            if (attributes.ContainsKey(attributeName))
            {
                try
                {
                    object value;
                    if (ConfigConvert.TryConvertFromString(typeof(T), attributes[attributeName], out value))
                        return (T)value;
                    return defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        public T GetOptionalAttribute<T>(string attributeName, T defaultValue, Func<string, T> converter)
        {
            if (attributes.ContainsKey(attributeName))
            {
                try
                {
                    return converter(attributes[attributeName]);
                }
                catch
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        void Parser()
        {
            foreach (var attr in element.Attributes())
            {
                try
                {
                    attributes.Add(attr.Name.LocalName, attr.Value);
                }
                catch (ArgumentException)
                {
                    throw new ConfigException("节点{0}中存在相同名称的属性：{1}", element.Name.LocalName, attr.Name.LocalName);
                }
            }
            children = element.Elements().Select(e => new ConfigElement(e)).ToArray();
        }
    }
}
