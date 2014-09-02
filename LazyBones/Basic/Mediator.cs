using LazyBones.Utils;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 用于数据的集中访问
    /// </summary>
    internal class Mediator
    {
        internal Mediator(DataAdapter dataProvider)
        {
            DataProvider = dataProvider;
            EmployeeManager = new Manager<Employee>(dataProvider);
            DepartmentManager = new Manager<Department>(dataProvider);    
        
            UserManager = new Manager<User>(dataProvider);
            RoleManager = new Manager<Role>(dataProvider);
            RoleResManager = new Manager<RoleRes>(dataProvider);
            UserRoleManager = new Manager<UserRole>(dataProvider);  
          
            Organization = new Organization(this);
            SecurityManager = new SecurityManager(this);

            Authenticator = new ADAuthenticator();
        }
        internal DataAdapter DataProvider { get; private set; }
        internal Organization Organization { get; private set; }
        internal IManager<Employee> EmployeeManager { get; private set; }
        internal IManager<Department> DepartmentManager { get; private set; }
        internal SecurityManager SecurityManager { get; private set; }
        internal IManager<User> UserManager { get; private set; }
        internal IManager<Role> RoleManager { get; private set; }
        internal IManager<RoleRes> RoleResManager { get; private set; }
        internal IManager<UserRole> UserRoleManager { get; private set; }
        EntityConverter entityConverter = EntityConverter.Default;
        internal EntityConverter Converter
        {
            get { return entityConverter; }
            set
            {
                ParamGuard.NotNull(value, "Converter");
                entityConverter = value;
            }
        }
        internal Authenticator Authenticator
        {
            get { return SecurityManager.Authenticator; }
            set
            {
                ParamGuard.NotNull(value, "Authenticator");
                SecurityManager.Authenticator = value;
            }
        }
    }
}
