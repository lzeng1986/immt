using System;

namespace LazyBones.Config
{
    /// <summary>
    /// 辅助类，提供对于AppDomain信息的查询功能
    /// </summary>
    public static class AppDomainWrapper
    {
        static AppDomain currentDomain;
        /// <summary>
        /// 获取当前AppDomain
        /// </summary>
        public static AppDomain Current
        {
            get
            {
                return currentDomain ?? (currentDomain = AppDomain.CurrentDomain);
            }
        }
        static string baseDirectory;
        /// <summary>
        /// 获取当前程序根目录
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                return baseDirectory ?? (baseDirectory = Current.BaseDirectory);
            }
        }
        static string configurationFile;
        /// <summary>
        /// 获取当前程序配置文件
        /// </summary>
        public static string ConfigurationFile
        {
            get
            {
                return configurationFile ?? (configurationFile = Current.SetupInformation.ConfigurationFile);
            }
        }
        static string[] privatePath;
        /// <summary>
        /// 获取当前程序根目录下的目录列表
        /// </summary>
        public static string[] PrivatePath
        {
            get
            {
                return privatePath ?? (privatePath = GetPriavtePath());
            }
        }
        static string[] GetPriavtePath()
        {
            var path = Current.SetupInformation.PrivateBinPath;
            if (string.IsNullOrEmpty(path))
            {
                return path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }
        static string friendName;
        /// <summary>
        /// 获取当前程序的友好名称
        /// </summary>
        public static string FriendlyName
        {
            get
            {
                return friendName ?? (friendName = Current.FriendlyName);
            }
        }
        /// <summary>
        /// 获取当前应用程序域下路径字符串的绝对路径，如果传入路径是完整路径，则直接返回
        /// <para>此函数与Path.GetFullPath的区别：</para>
        /// <para>对于一个相对路径，Path.GetFullPath不一定返回当前应用程序域下的路径，如windows服务</para>
        /// </summary>
        /// <param name="filePath">传入的路径</param>
        /// <returns>完整路径</returns>
        public static string GetFullPath(string filePath)
        {
            if (System.IO.Path.IsPathRooted(filePath))
                return filePath;
            return System.IO.Path.Combine(BaseDirectory, filePath);
        }
        /// <summary>
        /// 在应用程序退出时发生
        /// </summary>
        public static event EventHandler Exit
        {
            add
            {
                Current.DomainUnload += value;
                Current.ProcessExit += value;
            }
            remove
            {
                Current.DomainUnload -= value;
                Current.ProcessExit -= value;
            }
        }
    }
}
