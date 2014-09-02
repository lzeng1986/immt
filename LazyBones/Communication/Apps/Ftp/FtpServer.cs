using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LazyBones.Config;
using LazyBones.Log;
using LazyBones.Communication.Server;

namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpServer : IAppServer
    {
        static Logger logger = LogManager.Current;
        Dictionary<string, FtpUser> users = new Dictionary<string, FtpUser>();
        FileSystemDelayWatcher configWatch = new FileSystemDelayWatcher() { Delay = 5000 };

        public FtpServer()
        {
            PasvMinPort = 1000;
            PasvMaxPort = 2000;
            configWatch.FileChanged += new FileSystemEventHandler(configWatch_FileChanged);
        }

        void configWatch_FileChanged(object sender, FileSystemEventArgs e)
        {
            logger.Info("配置文件'{0}'被修改，开始重新加载", e.FullPath);
            LoadConfig(e.FullPath);
        }

        public int PasvMinPort { get; private set; }
        public int PasvMaxPort { get; private set; }
        public FtpUser GetUser(FtpSession session)
        {
            FtpUser user;
            if (!users.TryGetValue(session.User, out user))
            {
                logger.Info("未知用户{0}登录,form {1}", session.User, session.RemoteIPEP.ToString());
            }
            return user;
        }

        //protected override void OnStart()
        //{
        //    logger.Info("server '{0}' started", Name);
        //}

        //protected override void OnStop()
        //{
        //    logger.Info("server '{0}' stoped", Name);
        //}

        public void LoadConfig(string path)
        {
            try
            {
                configWatch.Stop();
                var ser = new XmlSerializer(typeof(FtpConfig));
                using (var reader = new StreamReader(AppDomainWrapper.GetFullPath(path)))
                {
                    var ftpConfig = ser.Deserialize(reader) as FtpConfig;
                    users.Clear();
                    PasvMinPort = ftpConfig.PasvMinPort;
                    PasvMaxPort = ftpConfig.PasvMaxPort;
                    if (ftpConfig.Policies != null)
                    {
                        foreach (var u in ftpConfig.Policies.Select(p => new FtpUser(p)))
                            users[u.User] = u;
                    }
                    if (ftpConfig.AutoReload)
                    {
                        configWatch.Watch(path);
                    }
                    logger.Info("配置文件加载成功");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "配置文件加载失败");
            }
        }

        public LBBinding Binding
        {
            get { return new FtpBinding(); }
        }

        public string Name
        {
            get { return "Ftp Server"; }
        }

        public void OnStart()
        {
            throw new NotImplementedException();
        }

        public void OnStop()
        {
            throw new NotImplementedException();
        }

        public string GetNewClientId(System.Net.IPEndPoint remoteIPEP)
        {
            throw new NotImplementedException();
        }
    }
}
