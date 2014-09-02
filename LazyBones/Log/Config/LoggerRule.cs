using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Linq;
using LazyBones.Log.Filters;
using LazyBones.Log.Targets;
using System.Text.RegularExpressions;

namespace LazyBones.Log.Config
{
    /// <summary>
    /// 表示记录器的记录规则，与配置文件中&lt;rule/&gt;配置项对应
    /// </summary>
    public class LoggerRule
    {
        readonly bool[] levelEnables = new bool[(int)LogLevel.Max - (int)LogLevel.Min + 1];   //表示每种日志级别是否记录
        IList<Target> targets = new List<Target>();
        IList<Filter> filters = new List<Filter>();
        IList<LoggerRule> childConfig = new List<LoggerRule>();
        string namePattern = string.Empty;
        string nameMatcherArg = string.Empty;
        string matchPattern = "^.*$"; // 默认可匹配所有记录器
        /// <summary>
        /// 创建一个默认的<see cref="LoggerRule"/>实例，此实例下不对任何级别的日志进行记录
        /// </summary>
        public LoggerRule()
        {
            IsFinal = false;
            MinLevel = LogLevel.Min;
            MaxLevel = LogLevel.Max;
            Enable = true;
        }
        /// <summary>
        /// 创建一个<see cref="LoggerRule"/>实例
        /// </summary>
        /// <param name="minLevel">可记录的最低日志级别，所有大于或等于此级别的日志都将被记录</param>
        public LoggerRule(LogLevel minLevel)
            : this()
        {
            for (var i = 0; i < levelEnables.Length; i++)
            {
                levelEnables[i] = (i >= minLevel);
            }
        }
        /// <summary>
        /// 指示开始记录某一日志级别
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        public void EnableLevel(LogLevel logLevel)
        {
            levelEnables[logLevel] = true;
        }
        /// <summary>
        /// 指示关闭记录某一日志级别
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        public void DisEnableLevel(LogLevel logLevel)
        {
            levelEnables[logLevel] = false;
        }
        /// <summary>
        /// 查询某一日志级别是否被记录
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns>是否被记录</returns>
        public bool CanLog(LogLevel logLevel)
        {
            return levelEnables[logLevel];
        }
        internal bool CanLog(int order)
        {
            return levelEnables[order];
        }
        /// <summary>
        /// 获取此配置允许记录日志级别的集合
        /// </summary>
        public LogLevel[] EnableLevels
        {
            get
            {
                return levelEnables.Where(i => i).Select((b, i) => (LogLevel)i).ToArray();
            }
        }

        /// <summary>
        /// 获取或设置此配置项可匹配的记录器名称，可使用'*'作为通配符
        /// </summary>
        public string NamePattern
        {
            get
            {
                return namePattern;
            }
            set
            {
                namePattern = value;
                if (string.IsNullOrEmpty(namePattern))
                    return;
                //使用正则表达式进行匹配
                matchPattern = '^' + namePattern.Replace("*", ".*") + '$';
            }
        }
        /// <summary>
        /// 检查此记录规则是否适用于指定记录器
        /// </summary>
        /// <param name="loggerName">记录器名称</param>
        /// <returns>是否匹配</returns>
        public bool CheckLoggerName(string loggerName)
        {
            return Regex.IsMatch(loggerName, matchPattern);
        }
        /// <summary>
        /// 获取或设置记录器配置是否为最后一个
        /// </summary>
        public bool IsFinal { get; set; }
        /// <summary>
        /// 获取或设置记录器配置记录最小<see cref="LogLevel"/>
        /// </summary>
        public LogLevel MinLevel { get; set; }
        /// <summary>
        /// 获取或设置记录器配置记录最大<see cref="LogLevel"/>
        /// </summary>
        public LogLevel MaxLevel { get; set; }
        /// <summary>
        /// 获取或设置记录器配置是否可用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 添加一个<see cref="Target"/>对象
        /// </summary>
        public void AddTarget(Target target)
        {
            targets.Add(target);
        }
        public IEnumerable<Target> Targets
        {
            get { return targets; }
        }
        /// <summary>
        /// 添加一个<see cref="Filter"/>对象
        /// </summary>
        public void AddFilter(Filter filter)
        {
            filters.Add(filter);
        }

        public override string ToString()
        {
            return string.Format("pattern:{0} enableLevels:{1} isFinal:{2} writeTo:{3}",
                NamePattern,
                EnableLevels.Select(l => l.Name).JoinToString("|"),
                IsFinal,
                targets.Select(t => t.Name).JoinToString("|"));
        }
    }
}
