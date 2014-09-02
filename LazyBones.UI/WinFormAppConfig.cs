using System;
using System.Collections.Generic;
using LazyBones.Basic;
using System.Linq;
using LazyBones.Utils;

namespace LazyBones.UI
{
    class WinFormAppConfig : IDisposable
    {
        public Type splashFormType;
        public List<MenuItemConfig> menuConfig;
        public InvalidMenuDisplayMode InvalidMenuDisplayMode = InvalidMenuDisplayMode.Hide;
        public void Dispose()
        {
            if (menuConfig != null)
            {
                menuConfig.ForEach(m => m.Dispose());
                menuConfig.Clear();
            }
            GC.SuppressFinalize(this);
        }
    }
    class MenuItemConfig : IDisposable
    {
        public MenuItemBase menuItemBase;
        public string name;
        public List<MenuItemConfig> childrenConfig = new List<MenuItemConfig>(0);
        public string[] validRoles;
        public bool ContainRole(Role role)
        {
            ParamGuard.NotNull(role, "role");
            if (validRoles == null)
                return true;
            return validRoles.Contains(role.Name);
        }
        public bool ContainRoles(IEnumerable<Role> roles)
        {
            if (roles == null)
                return false;
            if (validRoles == null)
                return true;
            return roles.Any(r => validRoles.Contains(r.Name));
        }
        public void Dispose()
        {
            childrenConfig.ForEach(m => m.Dispose());
            childrenConfig.Clear();
            validRoles = null;
            GC.SuppressFinalize(this);
        }
    }
    enum InvalidMenuDisplayMode
    {
        Hide,
        Disable
    }
}
