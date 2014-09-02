using System;
using System.Collections.Generic;
using System.Reflection;
using LazyBones.Config;
using LazyBones.Log.Filters;
using LazyBones.Log.Layouts;
using LazyBones.Log.Renderers;
using LazyBones.Log.Targets;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 提供日志项注册及生成实例的工厂
    /// </summary>
    class LogConfigItemFactory : IDisposable
    {
        readonly Factory<Target, TargetAttribute> targets = new Factory<Target, TargetAttribute>();
        readonly Factory<Filter, FilterAttribute> filters = new Factory<Filter, FilterAttribute>();
        readonly Factory<Layout, LayoutAttribute> layouts = new Factory<Layout, LayoutAttribute>();
        readonly Factory<Renderer, RendererAttribute> renderers = new Factory<Renderer, RendererAttribute>();
        IList<IFactory> factories;
        /// <summary>
        /// 生成一个新的<see cref="LogConfigItemFactory"/>实例
        /// </summary>
        public LogConfigItemFactory()
        {
            factories = new List<IFactory>{
                targets,
                filters,
                layouts,
                renderers
            };
        }
        /// <summary>
        /// 生成一个新的<see cref="LogConfigItemFactory"/>实例，并注册程序集
        /// </summary>
        /// <param name="assemblies">需要注册的程序集</param>
        public LogConfigItemFactory(params Assembly[] assemblies)
            : this()
        {
            if (assemblies == null)
                return;
            foreach (var ass in assemblies)
            {
                RegisterAssembly(ass, string.Empty);
            }
        }
        List<Assembly> registeredAssemblies = new List<Assembly>();
        public IList<Assembly> RegisteredAssemblies { get { return registeredAssemblies; } }
        /// <summary>
        /// 注册程序集
        /// </summary>
        public void RegisterAssembly(Assembly assembly, string prefix)
        {
            if (registeredAssemblies.Contains(assembly))
                return;
            foreach (var f in factories)
            {
                f.ScanAssembly(assembly, prefix);
            }
            registeredAssemblies.Add(assembly);
        }
        /// <summary>
        /// 获取创建<see cref="Target"/>的工厂
        /// </summary>
        public IItemFactory<Target> Targets
        {
            get { return targets; }
        }
        /// <summary>
        /// 获取创建<see cref="Filter"/>的工厂
        /// </summary>
        public IItemFactory<Filter> Filters
        {
            get { return filters; }
        }
        /// <summary>
        /// 获取创建<see cref="Layout"/>的工厂
        /// </summary>
        public IItemFactory<Layout> Layouts
        {
            get { return layouts; }
        }
        /// <summary>
        /// 获取创建<see cref="Renderer"/>的工厂
        /// </summary>
        public IItemFactory<Renderer> Renderers
        {
            get { return renderers; }
        }
        /// <summary>
        /// 释放配置项工厂包含的资源
        /// </summary>
        public void Dispose()
        {
            targets.Clear();
            filters.Clear();
            layouts.Clear();
            renderers.Clear();
            factories.Clear();
            registeredAssemblies.Clear();
            GC.SuppressFinalize(this);
        }

        static LogConfigItemFactory defaultFactory = null;
        /// <summary>
        /// 获取一个默认的日志项工厂
        /// </summary>
        public static LogConfigItemFactory DefaultFactory
        {
            get { return defaultFactory ?? (defaultFactory = new LogConfigItemFactory(typeof(LogConfigItemFactory).Assembly)); }
        }
    }
}
