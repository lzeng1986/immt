using System;

namespace LazyBones.Basic.Data
{
    /// <summary>
    /// 表明一个属性为主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class KeyAttribute : Attribute
    {
    }
}
