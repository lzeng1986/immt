using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 表示日志系统配置文件的一个元素
    /// </summary>
    public class LogXmlElement
    {
        readonly XElement element;
        readonly string elementName;
        readonly Dictionary<string, string> attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LogXmlElement[] children;
        
        /// <summary>
        /// 根据文件创建<see cref="LogXmlElement"/>实例
        /// </summary>
        /// <param name="fileName">文件名</param>
        public LogXmlElement(string fileName)
        {
            element = XElement.Load(fileName);
            Parser();
            elementName = element.Name.LocalName;
        }
        /// <summary>
        /// 根据<see cref="XElement"/>对象创建<see cref="LogXmlElement"/>实例
        /// </summary>
        /// <param name="element"><see cref="XElement"/>对象</param>
        public LogXmlElement(XElement element)
        {
            this.element = element;
            Parser();
            elementName = element.Name.LocalName;
        }
        /// <summary>
        /// 检查当前元素节点名称是否满足要求，检查过程忽略字母大小写
        /// </summary>
        /// <param name="expectNames">期望的元素名称</param>
        public void CheckName(params string[] expectNames)
        {
            if (expectNames.Any(n => n.Equals(elementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            throw new InvalidOperationException(
                "解析配置文件失败，节点名称应为：" + elementName + "，而此处为：" + string.Join("|", expectNames)
                );
        }
        /// <summary>
        /// 获取该节点的名称
        /// </summary>
        public string Name
        {
            get
            {
                return elementName;
            }
        }
        /// <summary>
        /// 获取节点所有的Attribute
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return attributes;
            }
        }
        /// <summary>
        /// 获取节点所有子节点
        /// </summary>
        public LogXmlElement[] Children
        {
            get
            {
                return children;
            }
        }
        /// <summary>
        /// 获取指定名称的子节点，忽略名称大小写
        /// </summary>
        /// <param name="name">子节点名称</param>
        /// <returns>获取的子节点</returns>
        public IEnumerable<LogXmlElement> Elements(string name)
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
            throw new LogConfigException("配置文件节点{0}缺少必须项{1}", elementName, attributeName);
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
                    return (T)Convert.ChangeType(attributes[attributeName], typeof(T));
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
                    throw new LogConfigException("节点{0}中存在相同名称的属性：{1}", 
                        element.Name.LocalName, attr.Name.LocalName);
                }                
            }
            var list = new List<LogXmlElement>();
            foreach (var e in element.Elements())
            {
                list.Add(new LogXmlElement(e));
            }
            children = list.ToArray();            
        }
    }
}
