using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LazyBones.Extensions;
using LazyBones.Log;
using LazyBones.Communication.Server;
using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpSession : IAppSession
    {
        static Logger logger = LogManager.Current;
        static IPAddress localIP = Dns.GetHostEntry("localhost").AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork);
        Func<Socket> connectionBuilder;
        ManualResetEvent transferWaitHandle = new ManualResetEvent(true);
        Dictionary<string, Action<string>> procMapper = new Dictionary<string, Action<string>>();

        public string User { get; private set; }
        public IPEndPoint RemoteIPEP { get; private set; }

        FtpServer ftpServer;
        FtpUser ftpUser;
        FtpDirectory ftpDir;

        bool isAuthenticated = false;
        
        public void Initialize()
        {
            ftpServer = OperationContext.Current.GetAppServerInstance<FtpServer>();
            procMapper = new Dictionary<string, Action<string>> { 
                {"LIST",LIST},
                {"NLST",NLST},
                {"NOOP",NOOP},
                {"PWD",PWD},
                {"PORT",PORT},
                {"PASV",PASV},
                {"CWD",CWD},
                {"TYPE",TYPE},
                {"SYST",SYST}
            };
            RemoteIPEP = OperationContext.Current.ContextSession.RemoteEndPoint;
            SendReply(220, ftpServer.Name + ", welcome!");
        }
        public void ProcessMessage(ILBMessage message)
        {
            Process(message as FtpMessage);
        }

        public void MessageSent(ILBMessage sentMessage)
        {
        }

        public void Dispose()
        {
        }

        void Process(FtpMessage message)
        {
            logger.Debug("recv : " + message.Text);
            var cmd = message.Cmd.ToUpper();
            if (cmd == "USER")
            {
                User = message.Value;
                ftpUser = ftpServer.GetUser(this);
                ftpDir = new FtpDirectory(ftpUser.Root);
                SendReply(331, "Password required!");
            }
            else if (cmd == "PASS")
            {
                if (ftpUser == null)
                {
                    SendReply(503, "Invalid User Name.");
                }
                else
                {
                    isAuthenticated = ftpUser.Authenticate(message.Value, RemoteIPEP.Address);
                    if(isAuthenticated)
                        SendReply(230, "Logon successful!");
                    else
                        SendReply(530, "Logon failed!");
                }
            }
            else if (ftpUser != null && isAuthenticated)
            {
                Action<string> handler;
                if (procMapper.TryGetValue(cmd, out handler))
                {
                    handler(message.Value);
                }
                else
                {
                    SendReply(500, "Command is unavailable currently!: " + message.Cmd);
                }
            }
            else
            {
                SendReply(530, "Failed! Log on first!");
            }
        }

        void SendReply(int cmd, string value)
        {
            var msg = new FtpMessage { Cmd = cmd.ToString(), Value = value };
            logger.Debug("send : " + msg.Text);
            OperationContext.Current.SendMessage(msg);
            //operation.SendMessage(msg);
        }

        void NOOP(string value)//无任何意义，只是验证服务是否可用
        {
            SendReply(200, "OK");
        }
        void PWD(string value)
        {
            SendReply(257, "\"" + ftpDir.CurrentWorkingDirectory.Replace("\\", "/") + "\"");//列出工作目录
        }
        void CWD(string value)//切换工作目录
        {
            if (ftpDir.ChangeWorkingDirectory(value))
            {
                SendReply(250, "Change working directory successful.");
            }
            else
            {
                SendReply(550, "Can't find directory '" + value + "'.");
            }
        }
        void SYST(string value)//打印当前操作系统名称
        {
            SendReply(215, Environment.OSVersion.ToString());
        }
        void TYPE(string value)
        {
            // I:Image 二进制;A:Ascii 文本模式
            //据网上资料，这两种模式的区别在于：Ascii模式会自动转换换行符，主要指切换Windows系统和Unix系统的换行符
            //Image模式则不会对数据做任何改动
            //本系统统一使用Image模式传输数据
            value = value.ToUpper();
            if (value == "I" || value == "A")
                SendReply(200, value + " Accepted.");
            else
                SendReply(500, "Unknown Type.");
        }
        void LIST(string value)
        {
            var dir = ftpDir.GetFullPath(value);
            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists)
            {
                SendReply(550, "Invalid path");
            }
            else if (!ftpUser.HasPermission(UserPermissions.ViewHideFloder) && dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                SendReply(550, "Invalid path");
            }
            else
            {
                TransferData((socket) =>
                {
                    var sb = new StringBuilder();
                    //经测试，LIST命令的格式只能为下述格式
                    foreach (var directory in dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        sb.AppendFormat("{0} <DIR> {1}\r\n", directory.CreationTime.ToString("MM-dd-yy hh:mmtt"), directory.Name);
                    }
                    foreach (var file in dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly))
                    {
                        sb.AppendFormat("{0} {1} {2}\r\n",
                            file.CreationTime.ToString("MM-dd-yy hh:mmtt"), file.Length, file.Name);
                    }
                    socket.Send(Encoding.Default.GetBytes(sb.ToString()));
                });
            }
        }
        void NLST(string value)
        {
            var dir = ftpDir.GetFullPath(value);
            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists)
            {
                SendReply(550, "Invalid path");
            }
            else if (!ftpUser.HasPermission(UserPermissions.ViewHideFloder) && dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
            {
                SendReply(550, "Invalid path");
            }
            else
            {
                TransferData((socket) =>
                {
                    var sb = new StringBuilder();
                    foreach (var directory in dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        sb.AppendLine(directory.Name);
                    }
                    socket.Send(Encoding.Default.GetBytes(sb.ToString()));
                });
            }
        }
        void Download(string value)
        {
            var path = Path.Combine(ftpDir.CurrentPath, value);
            if (!File.Exists(path))
            {
                SendReply(426, "Can't download from this directory.");
                logger.Info("用户{0}(from {1})尝试下载不存在的文件{2}", User, RemoteIPEP.ToString(), path);
            }
            else
            {
                TransferData((socket) =>
                {
                    var dataBuffer = new byte[1024];
                    var size = 0;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var len = fs.Length;
                        // 必须要在Send之前判断size是否为零
                        // 因为如果文件大小小于缓冲区，一次就可以传输完成，第二次传输时发生异常：远程主机强行关闭连接，errcode：10054
                        // 如果文件大于缓冲区，则不会发生此异常，具体原因不明[10/1/2013 zliang]
                        while (true)
                        {
                            size = fs.Read(dataBuffer, 0, dataBuffer.Length);
                            if (size <= 0 || !socket.Connected)
                            {
                                break;
                            }
                            socket.Send(dataBuffer, size, SocketFlags.None);
                        }
                    }
                });
            }
        }
        void Upload(string value)
        {
            //if (currentDir.Equals("input"))
            //{
            //    var path = Path.Combine(root, currentDir);
            //    path = Path.Combine(path, value);
            //    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            //    {
            //        TransferData((socket) =>
            //        {
            //            var dataBuffer = new byte[TransferBufferSize];
            //            var size = 0;
            //            while (true)
            //            {
            //                size = socket.Receive(dataBuffer, SocketFlags.None);
            //                if (size == 0)
            //                    break;
            //                fs.Write(dataBuffer, 0, size);
            //            }
            //            fs.Flush();
            //        });
            //    }
            //}
            //else
            //{
            //    SendReply(426, "Can't upload to this directory.");
            //}
        }
        void TransferData(Action<Socket> transfer)  //定义传输数据的流程
        {
            if (connectionBuilder == null)
            {
                SendReply(503, "Bad sequence of commands.");
            }
            else
            {
                try
                {
                    var socket = connectionBuilder();
                    transferWaitHandle.Reset();
                    try
                    {
                        transfer(socket);
                        socket.Shutdown(SocketShutdown.Both);
                        SendReply(226, "Transfer Complete.");
                    }
                    catch (System.Exception ex)
                    {
                        SendReply(426, "Connection closed unexpectedly.");
                        logger.Info("传输失败:" + ex.Message);
                    }
                    socket.Close();
                    transferWaitHandle.Set();
                }
                catch (TimeoutException)
                {
                    SendReply(425, "Data Connection Timed out.");
                    logger.Info("传输超时");
                }
                catch (Exception ex)
                {
                    SendReply(425, "Can't establish data connection!");
                    logger.Info("建立传输连接失败:" + ex.Message);
                }
                finally
                {
                    connectionBuilder = null;
                }
            }
        }
        void PASV(string value)
        {
            for (int port = ftpServer.PasvMinPort; port <= ftpServer.PasvMaxPort; port++)
            {
                try
                {
                    var pasvListener = new TcpListener(localIP, port);
                    var ep = string.Format("{0},{1},{2}", localIP.ToString(), port >> 8, port & 255).Replace('.', ',');
                    SendReply(227, "Entering Passive Mode (" + ep + ").");
                    pasvListener.Start();
                    connectionBuilder = () => PasvBuildConnection(pasvListener);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "建立PASV连接错误");
                }
            }
            SendReply(500, "Action Failed Retry.");
        }
        Socket PasvBuildConnection(TcpListener pasvListener)
        {
            try
            {
                for (int i = 0; i < 10; i++)    //等待10秒
                {
                    if (pasvListener.Pending())
                    {
                        var socket = pasvListener.AcceptSocket();
                        var ip = (socket.RemoteEndPoint as IPEndPoint).Address;
                        if (CheckIP(ip))
                        {
                            SendReply(125, "Connected, Starting Data Transfer.");
                        }
                        return socket;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                throw new TimeoutException();
            }
            finally
            {
                pasvListener.Stop();
                pasvListener = null;
            }
        }
        void PORT(string value)
        {
            var ipParts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ipParts.Length != 6)
            {
                SendReply(550, "Invalid arguments.");
            }
            else
            {
                var clientIP = string.Format("{0}.{1}.{2}.{3}", ipParts[0], ipParts[1], ipParts[2], ipParts[3]);
                var tmpPort = (Convert.ToInt32(ipParts[4]) << 8) | Convert.ToInt32(ipParts[5]);
                var portIPEP = new IPEndPoint(IPAddress.Parse(clientIP), tmpPort);
                //检查连接的ip地址是否与连接客户端的ip地址相同，这是一个对ftp协议简单的强化
                if (CheckIP(portIPEP.Address))
                {
                    SendReply(200, "Ready to connect.");
                    connectionBuilder = () => PortBuildConnection(portIPEP);
                }
            }
        }
        Socket PortBuildConnection(IPEndPoint portIPEP)
        {
            SendReply(150, "Connecting.");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Bind(new IPEndPoint(IPAddress.Any, Config.FtpPort - 1));
            socket.Connect(portIPEP);
            return socket;
        }
        bool CheckIP(IPAddress ip)//检查连接的ip地址是否与连接客户端的ip地址相同，这是一个对ftp协议简单的强化
        {
            //if (ip.ToString() != RemoteIPEP.Address.ToString())
            //{
            //    SendReply(550, "Invalid arguments.");
            //    return false;
            //}
            return true;
        }

        
    }
}
