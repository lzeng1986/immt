using System;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 基础框架，提供<see lang="static"/>方法和属性对公共数据集中
    /// </summary>
    public static class Framework
    {
        static Framework()
        {
        }
        /// <summary>
        /// 获取或设置框架验证器，设置验证器应在框架初始化之后进行
        /// </summary>
        public static Authenticator Authenticator
        {
            get { return mediator == null ? null : mediator.Authenticator; }
            set { Mediator.Authenticator = value; }
        }
        /// <summary>
        /// 初始化基础框架，设置数据源
        /// </summary>
        /// <param name="dataProvider">为基础框架设置的数据源</param>
        public static void Init(IDataSource dataSource)
        {
            mediator = new Mediator(new DataAdapter(dataSource));
        }
        /// <summary>
        /// 获取或设置实体转换器
        /// </summary>
        public static EntityConverter Converter
        {
            get { return mediator == null ? null : mediator.Converter; }
            set { Mediator.Converter = value; }
        }
        static Mediator mediator;
        /// <summary>
        /// 获取管理器接口
        /// </summary>
        static Mediator Mediator
        {
            get
            {
                if (mediator == null)
                    throw new InvalidOperationException("框架没有初始化");
                return mediator;
            }
        }
        /// <summary>
        /// 根据<see cref="User"/>对象获取<see cref="Employee"/>对象
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>对应的人员对象</returns>
        public static Employee GetEmployeeByUser(User user)
        {
            return Mediator.Converter.UserToEmployee<User,Employee>(user, Mediator.Organization);
        }
        /// <summary>
        /// 根据<see cref="Employee"/>对象获取<see cref="User"/>对象
        /// </summary>
        /// <param name="employee">人员对象</param>
        /// <returns>对应的用户对象</returns>
        public static User GetUserByEmployee(Employee employee)
        {
            return Mediator.Converter.EmployeeToUser<Employee, User>(employee, Mediator.SecurityManager);
        }
        /// <summary>
        /// 获取安全管理器
        /// </summary>
        public static SecurityManager Security
        {
            get { return Mediator.SecurityManager; }
        }
        /// <summary>
        /// 获取组织架构
        /// </summary>
        public static Organization Organization
        {
            get { return Mediator.Organization; }
        }
    }
}
