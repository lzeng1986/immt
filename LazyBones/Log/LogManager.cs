using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LazyBones.Config;

namespace LazyBones.Log
{
    /// <summary>
    /// 日志管理器，获取<see cref="Logger"/>实例
    /// <para>此日志系统提供类级别记录器</para>
    /// </summary>
    public static class LogManager
    {
        public static LogLevel GlobalMinLogLevel { get; internal set; }
        static LogFactory logFactory;
        static LogManager()
        {
            logFactory = new LogFactory();
            AppDomainWrapper.Exit += Exited;
            TinyLog.Info("日志系统启动");
        }
        static void Exited(object sender, EventArgs args)
        {
            if (logFactory != null)
                logFactory.Dispose();
            logFactory = null;
            TinyLog.Info("日志系统关闭");
        }
        /// <summary>
        /// 获取或设置日志系统是否抛出异常
        /// </summary>
        public static bool ThrowException
        {
            get { return logFactory.ThrowException; }
            set { logFactory.ThrowException = value; }
        }
        /// <summary>
        /// 获取当前类的记录器
        /// </summary>
        public static Logger Current
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                string loggerName;
                Type declaringType;
                int skipFrames = 1;
                do
                {
                    var frame = new StackFrame(skipFrames, false);
                    var method = frame.GetMethod();
                    declaringType = method.DeclaringType;
                    if (declaringType == null)
                    {
                        loggerName = method.Name;
                        break;
                    }
                    skipFrames++;
                    loggerName = declaringType.FullName;
                } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

                return logFactory.GetLogger(new LoggerKey(typeof(Logger), loggerName));
            }
        }

        /// <summary>
        /// 获取当前类指定类型的记录器
        /// </summary>
        /// <typeparam name="T">记录器类型，必须继承自<see cref="Logger"/>且必须实现参数为空的构造函数</typeparam>
        /// <returns>记录器</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T GetCurrentLogger<T>()
            where T : Logger, new()
        {
            string loggerName;
            Type declaringType;
            int framesToSkip = 1;
            do
            {
                var frame = new StackFrame(framesToSkip, false);
                var method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    loggerName = method.Name;
                    break;
                }
                framesToSkip++;
                loggerName = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));
            return (T)logFactory.GetLogger(new LoggerKey(typeof(T), loggerName));
        }
        /// <summary>
        /// 获取一个命名的记录器
        /// </summary>
        /// <param name="loggerName">记录器名称</param>
        /// <returns>记录器</returns>
        public static Logger GetLogger(string loggerName)
        {
            return logFactory.GetLogger(new LoggerKey(typeof(Logger), loggerName));
        }
        /// <summary>
        /// 打印当前日志系统信息
        /// </summary>
        /// <param name="output">输出目标</param>
        public static void Print(TextWriter output)
        {
            //添加ToList()是为了防止在枚举时logFactory自动重新加载，集合改变造成异常
            output.WriteLine("加载的程序集：");
            foreach (var ass in logFactory.Config.ItemFactory.RegisteredAssemblies.ToList())
                output.WriteLine(ass);

            output.WriteLine("加载的Targe类型：");
            foreach (var t in logFactory.Config.ItemFactory.Targets.RegisteredTypes.ToList())
                output.WriteLine(t);

            output.WriteLine("加载的Renderer类型：");
            foreach (var t in logFactory.Config.ItemFactory.Renderers.RegisteredTypes.ToList())
                output.WriteLine(t);

            output.WriteLine("加载的Layout类型：");
            foreach (var l in logFactory.Config.ItemFactory.Layouts.RegisteredTypes.ToList())
                output.WriteLine(l);

            output.WriteLine("加载的Targe：");
            foreach (var t in logFactory.Config.AllTargets.ToList())
                output.WriteLine(t);

            output.WriteLine("加载的LoggerRule：");
            foreach (var r in logFactory.Config.LoggerRules.ToList())
                output.WriteLine(r);
        }
        /// <summary>
        /// 与<see cref="Print"/>功能一致，但只在Debug模式下才会调用
        /// </summary>
        /// <param name="output">输出目标</param>
        [Conditional("DEBUG")]
        public static void DebugPrint(TextWriter output)
        {
            Print(output);
        }

        public static event EventHandler LogConfigChanged
        {
            add { logFactory.LogConfigChanged += value; }
            remove { logFactory.LogConfigChanged -= value; }
        }
    }
}
