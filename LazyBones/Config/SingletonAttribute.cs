using System;

namespace LazyBones.Config
{
    /// <summary>
    /// 添加此特性的类在应用程序整个生命周期里只会存在一个对象
    /// <para>这通常用来标记不需要添加额外配置的对象</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
    }
}
