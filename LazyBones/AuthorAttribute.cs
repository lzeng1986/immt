using System;

namespace LazyBones
{
    /// <summary>
    /// 用来标识代码的作者
    /// 使用这个特性可以保证在编译之后的exe或者dll文件中，依然可以明文查看作者信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class AuthorAttribute : Attribute
    {
        readonly string author;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="author">作者名称，如有多个，请用';'隔开</param>
        public AuthorAttribute(string author)
        {
            this.author = author;
        }
        /// <summary>
        /// 获取作者名称
        /// </summary>
        public string Author { get { return this.author; } }
    }
}
