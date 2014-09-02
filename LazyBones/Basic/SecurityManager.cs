using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 安全管理器，提供对安全信息访问的接口
    /// </summary>
    public class SecurityManager
    {
        Mediator mediator;
        internal SecurityManager(Mediator mediator)
        {
            this.mediator = mediator;
            Authenticator = new ADAuthenticator();
        }
        /// <summary>
        /// 获取用户管理器
        /// </summary>
        public IManager<User> UserManager
        {
            get { return mediator.UserManager; }
        }
        /// <summary>
        /// 获取角色管理器
        /// </summary>
        public IManager<Role> RoleManager
        {
            get { return mediator.RoleManager; }
        }
        /// <summary>
        /// 获取用户角色管理器
        /// </summary>
        public IManager<UserRole> UserRoleManager
        {
            get { return mediator.UserRoleManager; }
        }
        /// <summary>
        /// 获取角色资源管理器
        /// </summary>
        public IManager<RoleRes> RoleResManager
        {
            get { return mediator.RoleResManager; }
        }
        /// <summary>
        /// 获取验证器
        /// </summary>
        public Authenticator Authenticator { get; internal set; }
        /// <summary>
        /// 添加一个用户角色
        /// </summary>
        public void AddUserToRole(string userName, string roleName)
        {
            AddUserToRole(new UserRole { UserName = userName, RoleName = roleName });
        }
        /// <summary>
        /// 添加一个<see cref="UserRole"/>对象
        /// </summary>
        [Res(SecurityAction.Demand, ResName = "添加用户角色")]
        public void AddUserToRole(UserRole userRole)
        {
            mediator.UserRoleManager.Add(userRole);
        }
        /// <summary>
        /// 删除一个<see cref="UserRole"/>对象
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="roleName">角色名</param>
        [Res(SecurityAction.Demand, ResName = "删除用户角色")]
        public void RemoveUserRole(string userName, string roleName)
        {
            RemoveUserRole(new UserRole { UserName = userName, RoleName = roleName });
        }
        /// <summary>
        /// 删除一个<see cref="UserRole"/>对象
        /// </summary>
        /// <param name="userRole">删除的<see cref="UserRole"/>对象</param>
        [Res(SecurityAction.Demand, ResName = "删除用户角色")]
        public void RemoveUserRole(UserRole userRole)
        {
            mediator.UserRoleManager.Remove(userRole);
        }
        /// <summary>
        /// 添加一个授权关系
        /// </summary>
        /// <param name="roleName">授权关系的角色名</param>
        /// <param name="resource">授权关系的资源</param>
        public void AddRoleRes(string roleName, string resource)
        {
            AddRoleRes(new RoleRes { RoleName = roleName, Resource = resource });
        }
        /// <summary>
        /// 添加一个<see cref="RoleRes"/>
        /// </summary>
        /// <param name="relation">添加的授权关系</param>
        [Res(SecurityAction.Demand, ResName = "添加授权关系")]
        public void AddRoleRes(RoleRes relation)
        {
            mediator.RoleResManager.Add(relation);
        }
        /// <summary>
        /// 删除一个<see cref="RoleRes"/>
        /// </summary>
        /// <param name="relation">删除的授权关系</param>
        [Res(SecurityAction.Demand, ResName = "删除授权关系")]
        public void RemoveRelation(RoleRes relation)
        {
            mediator.RoleResManager.Remove(relation);
        }
        /// <summary>
        /// 根据用户名和密码登录系统
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录结果</returns>
        public OpResult Login(string userName, string password)
        {
            return Authenticator.Login(userName, password);
        }
        /// <summary>
        /// 用户登出系统
        /// </summary>
        public void Logout()
        {
            //未实现
        }
        /// <summary>
        /// 判断当前用户是否已授权指定资源
        /// </summary>
        /// <param name="resource">授权资源</param>
        /// <returns>授权结果</returns>
        public OpResult Authorize(string resource)
        {
            if (ValidResources.Contains(resource))
                return new OpResult(true, "授权成功");
            return new OpResult(false, "用户没有访问当前资源权限");
        }
        User user = null;
        /// <summary>
        /// 获取当前用户
        /// </summary>
        public User User
        {
            get
            {
                if (user == null)
                {
                    if (!Authenticator.IsAuthenticated)
                    {
                        throw new InvalidOperationException();
                    }
                    var name = Authenticator.Name;
                    user = UserManager.FirstOrDefault(u => u.UserName == name);
                    if (user == null)
                        throw new SecurityException("用户<" + name + ">不是系统用户");
                }
                return user;
            }
        }
        string[] roles = null;
        /// <summary>
        /// 获取当前用户的角色列表
        /// </summary>
        public string[] Roles
        {
            get
            {
                if (roles == null)
                {
                    var userName = User.UserName;
                    roles = UserRoleManager
                        .Where(o => o.UserName == userName)
                        .Select(o => o.RoleName).ToArray();
                }
                return roles;
            }
        }
        string[] validResources = null;
        /// <summary>
        /// 获取当前用户可访问资源列表
        /// </summary>
        public string[] ValidResources
        {
            get
            {
                if (validResources == null)
                {
                    validResources = Roles.SelectMany(
                        r => RoleResManager.Where(roleRes => roleRes.RoleName == r)
                        ).Select(r => r.Resource).ToArray();
                }
                return validResources;
            }
        }
        /// <summary>
        /// 根据用户名获取对应用户
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>对应用户，不存在则返回<see lang="null"/></returns>
        public User GetUser(string userName)
        {
            return mediator.UserManager.FirstOrDefault(u => u.UserName == userName);
        }
    }
}
