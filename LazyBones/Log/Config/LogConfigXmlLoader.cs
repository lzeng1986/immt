using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using LazyBones.Config;
using LazyBones.Log.Core;
using LazyBones.Log.Targets;
using LazyBones.Log.Targets.Wrappers;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 用于从xml文档加载配置，生成相应的<see cref="LogConfig"/>
    /// </summary>
    public class LogConfigXmlLoader : IDisposable
    {
        readonly HashSet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, string> vars = new Dictionary<string, string>();
        LogConfigItemFactory logItemFactory = LogConfigItemFactory.DefaultFactory;
        LogConfig logConfig;

        /// <summary>
        /// 加载xml文件，如果是相对路径，则从应用程序根目录开始搜索并加载
        /// </summary>
        /// <param name="fileName">文件名称</param>
        public LogConfig Load(string fileName)   //解析xml配置文件
        {
            try
            {
                logConfig = new LogConfig();
                ParserFile(fileName);
                logConfig.ItemFactory = logItemFactory;
                return logConfig;
            }
            finally
            {
                logItemFactory = null;
                logConfig = null;
            }
        }
        void ParserFile(string path) // 解析配置文件
        {
            path = AppDomainWrapper.GetFullPath(path);
            if (!File.Exists(path))
            {
                TinyLog.Error("配置文件<{0}>不存在。", path);
                return;
            }
            if (files.Contains(path))   //由于配置文件<using/>中可能包含重复文件，因此需要添加过滤
            {
                return;
            }
            files.Add(path);
            logConfig.AddConfigFile(path);
            var log = new ConfigElement(XElement.Load(path));
            ParserLogElement(log, path);
        }
        //解析<log>节点
        internal void ParserLogElement(ConfigElement logElement, string filePath)
        {
            var baseDirectory = Path.GetDirectoryName(filePath);
            foreach (var element in logElement.Children)
            {
                try
                {
                    switch (element.Name.ToLowerInvariant())
                    {
                        case "global":
                            ParseGlobalElement(element);
                            break;
                        case "dlls":
                            ParseDllsElement(element, baseDirectory);
                            break;
                        case "vars":
                            ParseVarsElement(element);
                            break;
                        case "targets":
                            ParseTargetsElement(element);
                            break;
                        case "rules":
                            ParseRulesElement(element);
                            break;
                        default:
                            TinyLog.Warn("未知别的配置项<{0}>。文件<{1}>", element.Name, filePath);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    TinyLog.Error("解析配置项<{0}>失败，配置文件<{1}>，错误：{2}", element.Name, filePath, ex.Message);
                }
            }
        }
        void ParseGlobalElement(ConfigElement globalElement)
        {
            TinyLog.LogToConsole = globalElement.GetOptionalAttribute("tinyLog.LogToConsole", TinyLog.LogToConsole);
            TinyLog.LogToConsoleError = globalElement.GetOptionalAttribute("tinyLog.LogToConsoleError", TinyLog.LogToConsoleError);
            TinyLog.LogFile = globalElement.GetOptionalAttribute("tinyLog.LogFile", TinyLog.LogFile);
            TinyLog.MinLogLevel = globalElement.GetOptionalAttribute("tinyLog.MinLogLevel", TinyLog.MinLogLevel);
            LogManager.GlobalMinLogLevel = globalElement.GetOptionalAttribute("globalMinLogLevel", LogLevel.Info);
            logConfig.AutoReload = globalElement.GetOptionalAttribute("autoReload", true);
        }
        void ParseVarsElement(ConfigElement varsElement)
        {
            varsElement.CheckName("vars");
            foreach (var item in varsElement.Elements("var"))
            {
                var id = item.GetRequiredAttribute("id");
                if (vars.ContainsKey(id))
                {
                    throw new LogConfigException("配置文件中已存在同名变量：" + id);
                }
                var value = item.GetRequiredAttribute("value");
                value = AssignVarValue(value);
                vars.Add(id, value);
            }
        }
        void ParseDllsElement(ConfigElement importElement, string baseDirectory)  //解析<import/>节点
        {
            importElement.CheckName("dlls");
            foreach (var dll in importElement.Elements("add"))
            {
                var path = dll.GetRequiredAttribute("file");
                var namaspace = dll.GetRequiredAttribute("namaspace");
                try
                {
                    path = AppDomainWrapper.GetFullPath(path);
                    var assembly = Assembly.LoadFile(path);
                    logItemFactory.RegisterAssembly(assembly, namaspace);
                }
                catch (System.Exception ex)
                {
                    ex.Filter();
                    TinyLog.Error("加载扩展Dll<{0}>时出现错误:{1}", path, ex);
                }
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
                logConfig.AddTarget(target.Name, target);
            }
        }
        TargetBase ParserTargetElement(ConfigElement configElement)
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
            return target;
        }
        void ParseRulesElement(ConfigElement loggersElement)  //解析<rules/>节点
        {
            loggersElement.CheckName("rules");
            foreach (var l in loggersElement.Elements("rule"))
            {
                ParserRuleElement(l);
            }
        }
        void ParserRuleElement(ConfigElement loggerElement)    //解析<logger/>节点
        {
            loggerElement.CheckName("rule");
            var enable = loggerElement.GetOptionalAttribute("enable", true);
            if (!enable)
            {
                return;
            }
            var loggerRule = new LoggerRule();
            loggerRule.NamePattern = loggerElement.GetOptionalAttribute("pattern", "*");
            var level = loggerElement.GetOptionalAttribute("levels", "debug,info,warn,error,fatal")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in level)
            {
                loggerRule.EnableLevel((LogLevel)l);
            }
            loggerRule.IsFinal = loggerElement.GetOptionalAttribute("isFinal", false);

            var targets = loggerElement.GetOptionalAttribute("writeTo", (string)null);
            if (targets != null)
            {
                foreach (var targetName in targets.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var target = logConfig.GetTarget(targetName);
                    if (target == null)
                    {
                        throw new LogConfigException("未找到名为<{0}>的Target", targetName);
                    }
                    loggerRule.AddTarget(target);
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
                        ParserRuleElement(e);
                        break;
                    default:
                        throw new LogConfigException("发现不可识别的节点：" + e.Name);
                }
            }

            logConfig.AddLogger(loggerRule);
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
            foreach (var v in vars)
            {
                value = value.Replace("{" + v.Key + "}", v.Value);
            }
            return value;
        }
        /// <summary>
        /// 释放加载器的资源
        /// </summary>
        public void Dispose()
        {
            files.Clear();
            vars.Clear();
        }
    }
}
