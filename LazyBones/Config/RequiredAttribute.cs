using System;

namespace LazyBones.Config
{
    /// <summary>
    /// 用于标记一个属性必须要赋值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class RequiredAttribute : Attribute
    {
    }
}
