using System.Linq;
using LazyBones.Utils;

namespace LazyBones.Basic
{
    /// <summary>
    /// 实体转换器，用于安全实体与组织架构实体之间的转换
    /// </summary>
    public abstract class EntityConverter
    {
        private class DefaultConverter : EntityConverter
        {
            public override TUser EmployeeToUser<TEmployee, TUser>(TEmployee employee, SecurityManager securityManager)
            {
                ParamGuard.NotNull(employee, "employee");
                return (TUser)securityManager.GetUser(employee.Id);
            }

            public override TEmployee UserToEmployee<TUser, TEmployee>(TUser user, Organization organization)
            {
                ParamGuard.NotNull(user, "user");
                return (TEmployee)organization.GetEmployee(user.UserName);
            }
        }
        /// <summary>
        /// 默认的实体转换器，此默认转换器假设<see cref="User.UserName"/>与<see cref="Employee.Id"/>相等，并以此完成转换
        /// </summary>
        public static EntityConverter Default = new DefaultConverter();
        /// <summary>
        /// 将<see cref="Employee"/>对象转换为等价的<see cref="User"/>对象
        /// </summary>
        public abstract TUser EmployeeToUser<TEmployee, TUser>(TEmployee employee, SecurityManager securityManager)
            where TEmployee : Employee
            where TUser : User;
        /// <summary>
        /// 将<see cref="User"/>对象转换为等价的<see cref="Employee"/>对象
        /// </summary>
        public abstract TEmployee UserToEmployee<TUser, TEmployee>(TUser user, Organization organization)
            where TUser : User
            where TEmployee : Employee;
    }
}
