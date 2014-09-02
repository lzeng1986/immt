using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Log.Targets;
using LazyBones.Config;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using LazyBones.Log.Targets.Wrappers;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 日志系统配置
    /// </summary>
    public class LogConfig : IDisposable
    {
        IDictionary<string, Target> targets = new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);
        IList<LoggerRule> loggerRules = new List<LoggerRule>();
        List<string> configFiles = new List<string>();
        Dictionary<string, string> variableMap = new Dictionary<string, string>();
        LogConfigItemFactory logItemFactory = new LogConfigItemFactory(typeof(LogConfig).Assembly);
        private LogConfig()
        {
            ThrowException = false;
            AutoReload = false;
        }
        /// <summary>
        /// 根据名称获取对应的<see cref="Target"/>
        /// </summary>
        /// <param name="targetName">名称</param>
        /// <returns>对应的<see cref="Target"/></returns>
        public Target GetTarget(string targetName)
        {
            Target target = null;
            targets.TryGetValue(targetName, out target);
            return target;
        }
        /// <summary>
        /// 获取该配置加载的所有<see cref="LoggerRule"/>
        /// </summary>
        public IList<LoggerRule> LoggerRules
        {
            get { return loggerRules; }
        }
        /// <summary>
        /// 获取该配置加载的所有<see cref="Target"/>
        /// </summary>
        public Target[] AllTargets
        {
            get { return targets.Values.ToArray(); }
        }
        /// <summary>
        /// 获取该配置所加载的配置文件列表
        /// </summary>
        public string[] ConfigFiles
        {
            get { return configFiles.ToArray(); }
        }
        /// <summary>
        /// 释放所有已加载日志配置资源
        /// </summary>
        public void Dispose()
        {
            foreach (var target in targets.Values)
                target.Dispose();
            targets.Clear();
            loggerRules.Clear();
            configFiles.Clear();
            GC.SuppressFinalize(this);
        }
        internal LogConfigItemFactory ItemFactory
        {
            get { return logItemFactory; }
        }
        public bool ThrowException { get; set; }
        /// <summary>
        /// 获取或设置日志系统在配置文件改变时是否自动加载
        /// </summary>
        public bool AutoReload { get; set; }

        /// <summary>
        /// 加载xml文件，如果是相对路径，则从应用程序根目录开始搜索并加载
        /// </summary>
        /// <param name="fileName">文件名称</param>
        public static LogConfig Load(string fileName)   //解析xml配置文件
        {
            var logConfig = new LogConfig();
            logConfig.ParserFile(fileName);
            logConfig.Initialize();
            return logConfig;
        }
        void ParserFile(string path) // 解析配置文件
        {
            path = AppDomainWrapper.GetFullPath(path);
            if (!File.Exists(path))
            {
                throw new LogConfigException("配置文件不存在，path:{0}", path);
            }
            if (configFiles.Contains(path))   //由于配置文件<using/>中可能包含重复文件，因此需要添加过滤
            {
                return;
            }
            configFiles.Add(path);
            var log = new ConfigElement(XElement.Load(path));
            ParserLogElement(log, path);
        }
        void Initialize()
        {
            foreach (var target in targets)
            {
                target.Value.Initialize(this);
            }
        }
        static string[] ParserOrder = new[] { "global", "dlls", "vars", "targets", "loggers" };
        //解析<log>节点
        void ParserLogElement(ConfigElement logElement, string filePath)
        {
            logElement.CheckName("log");
            //需保证节点按照ParserOrder的顺序进行解析，否则可能出现错误
            var baseDirectory = Path.GetDirectoryName(filePath);
            var orderedChildren = logElement.Children
                .GroupBy(e => Array.IndexOf(ParserOrder, e.Name))
                .OrderBy(e => e.Key);
            var invalidElements = new List<ConfigElement>(0);
            foreach (var element in orderedChildren)
            {
                if (element.Key == -1)
                {
                    invalidElements.AddRange(element);
                }
                else
                {
                    var name = ParserOrder[element.Key];
                    if (element.Count() > 1)
                        throw new LogConfigException("文件<{0}>存在多个{1}配置", filePath, name);
                    try
                    {
                        switch (name)
                        {
                            case "global":
                                ParseGlobalElement(element.First());
                                break;
                            case "dlls":
                                ParseDllsElement(element.First(), baseDirectory);
                                break;
                            case "vars":
                                ParseVarsElement(element.First());
                                break;
                            case "targets":
                                ParseTargetsElement(element.First());
                                break;
                            case "loggers":
                                ParseLoggersElement(element.First());
                                break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ex.Check();
                        TinyLog.Error("配置文件\"{0}\"配置项\"{1}\"解析失败：{2}", filePath, name, ex.Message);
                    }
                }
            }
            invalidElements.ForEach(e=>TinyLog.Warn("文件<{0}>未知别的配置项<{1}>", filePath, e.Name));
        }
        void ParseGlobalElement(ConfigElement globalElement)
        {
            globalElement.CheckName("global");
            TinyLog.LogToConsole = globalElement.GetOptionalAttribute("tinyLog.LogToConsole", TinyLog.LogToConsole);
            TinyLog.LogToConsoleError = globalElement.GetOptionalAttribute("tinyLog.LogToConsoleError", TinyLog.LogToConsoleError);
            TinyLog.LogFile = globalElement.GetOptionalAttribute("tinyLog.LogFile", TinyLog.LogFile);
            TinyLog.MinLogLevel = globalElement.GetOptionalAttribute("tinyLog.MinLogLevel", TinyLog.MinLogLevel);
            LogManager.GlobalMinLogLevel = globalElement.GetOptionalAttribute("globalMinLogLevel", LogLevel.Info);
            AutoReload = globalElement.GetOptionalAttribute("autoReload", true);
            ThrowException = globalElement.GetOptionalAttribute("throwException", false);
        }
        void ParseDllsElement(ConfigElement importElement, string baseDirectory)  //解析<import/>节点
        {
            importElement.CheckName("dlls");
            foreach (var dll in importElement.Elements("add"))
            {
                var path = dll.GetRequiredAttribute("file");
                var @namespace = dll.GetRequiredAttribute("namespace");
                try
                {
                    path = AppDomainWrapper.GetFullPath(path);
                    var assembly = Assembly.LoadFile(path);
                    logItemFactory.RegisterAssembly(assembly, @namespace);
                }
                catch (System.Exception ex)
                {
                    ex.Check();
                    TinyLog.Error("加载扩展Dll<{0}>时出现错误:{1}", path, ex);
                }
            }
        }
        void ParseVarsElement(ConfigElement varsElement)
        {
            varsElement.CheckName("vars");
            foreach (var item in varsElement.Elements("add"))
            {
                var id = item.GetRequiredAttribute("id");
                if (variableMap.ContainsKey(id))
                {
                    throw new LogConfigException("配置文件中已存在同名变量：" + id);
                }
                var value = item.GetRequiredAttribute("value");
                value = AssignVarValue(value);
                variableMap.Add(id, value);
            }
        }
        void ParseTargetsElement(ConfigElement targetsElement)
        {
            targetsElement.CheckName("targets");
            var isAsync = targetsElement.GetOptionalAttribute("async", false);

            foreach (var e in targetsElement.Elements("target"))
            {
                var target = ParserTargetElement(e);
                if (isAsync)
                {
                    target = new TargetAsyncWrapper(target);
                }
                targets.Add(target.Name, target);
            }
        }
        Target ParserTargetElement(ConfigElement configElement)
        {
            var targetType = configElement.GetRequiredAttribute("type");
            var target = logItemFactory.Targets.GetInstance(targetType);
            Helper.ResetPropertyValue(target, logItemFactory);
            target.Name = configElement.GetRequiredAttribute("name");
            foreach (var attr in configElement.Attributes)
            {
                if (attr.Key == "type" || attr.Key == "name")
                    continue;
                var value = AssignVarValue(attr.Value);
                Helper.SetPropertyFromString(target, attr.Key, value, logItemFactory);
            }
            Helper.Validate(target);
            return target;
        }
        void ParseLoggersElement(ConfigElement loggersElement)  //解析<loggers/>节点
        {
            loggersElement.CheckName("loggers");
            foreach (var l in loggersElement.Elements("logger"))
            {
                ParserLoggerElement(l);
            }
        }
        void ParserLoggerElement(ConfigElement loggerElement)    //解析<logger/>节点
        {
            loggerElement.CheckName("logger");
            var enable = loggerElement.GetOptionalAttribute("enable", true);
            if (!enable)
            {
                return;
            }
            var loggerRule = new LoggerRule();
            loggerRule.NamePattern = loggerElement.GetRequiredAttribute("name");
            loggerRule.MinLevel = loggerElement.GetOptionalAttribute("minLevel", LogLevel.Min);
            loggerRule.MaxLevel = loggerElement.GetOptionalAttribute("maxLevel", LogLevel.Max);
            if (loggerRule.MinLevel > loggerRule.MaxLevel)
                throw new LogConfigException("rule(pattern={0})中MinLevel大于MaxLevel", loggerRule.NamePattern);

            for (var l = loggerRule.MinLevel; l <= loggerRule.MaxLevel; l++)
                loggerRule.EnableLevel(l);

            loggerRule.IsFinal = loggerElement.GetOptionalAttribute("isFinal", false);

            var writeToTargets = loggerElement.GetOptionalAttribute("writeTo", (string)null);
            if (writeToTargets != null)
            {
                foreach (var name in writeToTargets.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Target target;
                    if (targets.TryGetValue(name, out target))
                    {
                        loggerRule.AddTarget(target);
                    }
                    else
                    {
                        throw new LogConfigException("未找到名为\"{0}\"的Target", name);
                    }
                }
            }
            foreach (var e in loggerElement.Children)
            {
                switch (e.Name.ToLowerInvariant())
                {
                    case "filters":
                        ParserFiltersElement(loggerRule, e);
                        break;
                    case "logger":
                        ParserLoggerElement(e);
                        break;
                    default:
                        throw new LogConfigException("发现不可识别的节点：" + e.Name);
                }
            }
            loggerRules.Add(loggerRule);
        }
        void ParserFiltersElement(LoggerRule logger, ConfigElement filtersElement)
        {

        }
        void ParserFilterElement(ConfigElement filterElement)
        {

        }

        //如存在变量，则替换变量，变量id区分大小写
        string AssignVarValue(string value)
        {
            foreach (var v in variableMap)
            {
                value = value.Replace("{" + v.Key + "}", v.Value);
            }
            return value;
        }
    }
}
