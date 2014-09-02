using System;
using System.Security;
using System.Security.Permissions;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表明该对象为资源，需要进行权限检查
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Method |
        AttributeTargets.Constructor |
        AttributeTargets.Property |
        AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public sealed class ResAttribute : CodeAccessSecurityAttribute
    {
        /// <summary>
        /// 获取或设置资源名称
        /// </summary>
        public string ResName { get; set; }
        /// <summary>
        /// 创建一个<see cref="ResAttribute"/>对象
        /// </summary>
        public ResAttribute(SecurityAction action)
            : base(SecurityAction.Demand)
        {
        }
        /// <summary>
        /// 创建并返回一个新的<see cref="IPermission"/>对象
        /// </summary>
        /// <returns>与此资源对应的<see cref="IPermission"/>对象</returns>
        public override IPermission CreatePermission()
        {
            return new InternalPermission(ResName);
        }
    }

    internal class InternalPermission : IPermission, IUnrestrictedPermission
    {
        public bool Unrestricted { get; private set; }

        string resName;
        internal InternalPermission(string resName)
        {
            this.resName = resName;
            Unrestricted = true;
        }

        public IPermission Copy()
        {
            return this;
        }

        public void Demand()
        {
            var result = Framework.Security.Authorize(resName);
            if (result.IsSuccess)
                return;
            throw new SecurityException("权限不足");
        }

        public IPermission Intersect(IPermission target)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IPermission target)
        {
            return false;
        }

        public IPermission Union(IPermission target)
        {
            throw new NotImplementedException();
        }

        public void FromXml(SecurityElement e)
        {
            throw new NotImplementedException();
        }

        public SecurityElement ToXml()
        {
            throw new NotImplementedException();
        }

        public bool IsUnrestricted()
        {
            return Unrestricted;
        }
    }
}
