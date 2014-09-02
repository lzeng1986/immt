using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Config;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using LazyBones.Extensions;

namespace LazyBones.UI
{
    /// <summary>
    /// 用于从配置文件中加载WinForm配置
    /// </summary>
    class WinFormConfigXmlLoader : IDisposable
    {
        WinFormAppConfig appConfig;
        Dictionary<string, MenuItemBase> menuItems = new Dictionary<string, MenuItemBase>();
        public WinFormAppConfig Load(string filePath)
        {
            filePath = AppDomainWrapper.GetFullPath(filePath);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("未找到必须的配置文件" + filePath);

            var entryAss = Assembly.GetEntryAssembly();
            if (entryAss != null)
                RegisterAssembly(entryAss, "");

            appConfig = new WinFormAppConfig();
            var configElement = new ConfigElement(XElement.Load(filePath));
            configElement.CheckName("winForm");
            foreach (var e in configElement.Children)
            {
                switch (e.Name.ToLowerInvariant())
                {
                    case "import":
                        LoadImportConfig(e);
                        break;
                    case "app":
                        LoadAppConfig(e);
                        break;
                    case "menu":
                        LoadMenuConfig(e);
                        break;
                }
            }
            try
            {
                return appConfig;
            }
            finally
            {
                appConfig = null;
            }
        }
        void RegisterAssembly(Assembly assembly, string prefix)
        {
            foreach (var type in assembly.GetTypeSafe().Where(t => t.IsSubclassOf(typeof(MenuItemBase))))
            {
                var attrs = (MenuItemIdAttribute[])type.GetCustomAttributes(typeof(MenuItemIdAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                menuItems.Add(prefix + attrs[0].MenuName, (MenuItemBase)Activator.CreateInstance(type));
            }
        }
        void LoadImportConfig(ConfigElement importConfig)
        {
            importConfig.CheckName("import");
            foreach (var ass in importConfig.Elements("add"))
            {
                var path = ass.GetRequiredAttribute("dll");
                path = AppDomainWrapper.GetFullPath(path);
                var assembly = Assembly.LoadFile(path);
                var prefix = ass.GetOptionalAttribute("prefix", "");
                RegisterAssembly(assembly, prefix);
            }
        }
        void LoadAppConfig(ConfigElement appConfig)
        {
            appConfig.CheckName("app");
        }
        void LoadMenuConfig(ConfigElement menuConfig)
        {
            menuConfig.CheckName("menu");
            appConfig.menuConfig = new List<MenuItemConfig>();
            foreach (var item in menuConfig.Elements("menuItem"))
            {
                appConfig.menuConfig.Add(LoadItemConfig(item));
            }

            var mode = menuConfig.GetOptionalAttribute("invalidMenuDisplayMode", "hide");
            switch (mode.ToLowerInvariant())
            {
                case "hide":
                    appConfig.InvalidMenuDisplayMode = InvalidMenuDisplayMode.Hide;
                    break;
                case "disable":
                    appConfig.InvalidMenuDisplayMode = InvalidMenuDisplayMode.Disable;
                    break;
            }
        }
        MenuItemConfig LoadItemConfig(ConfigElement itemConfig)
        {
            itemConfig.CheckName("menuItem");
            var name = itemConfig.GetRequiredAttribute("id");
            MenuItemBase menuItemBase = null;
            if (menuItems.TryGetValue(name, out menuItemBase))
            {
                var menuItem = new MenuItemConfig();
                menuItem.menuItemBase = menuItemBase;
                menuItem.name = name;
                menuItem.childrenConfig.AddRange(itemConfig.Elements("item").Select(i=>LoadItemConfig(i)));
                return menuItem;
            }
            else
            {
                throw new ConfigException("为找到名为<{0}>的菜单项", name);
            }
        }
        public void Dispose()
        {
            menuItems.Clear();
        }
    }
}
