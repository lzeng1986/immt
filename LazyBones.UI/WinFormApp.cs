using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LazyBones.UI
{
    /// <summary>
    /// 提供管理WinForm应用程序的方法
    /// </summary>
    public static class WinFormApp
    {
        /// <summary>
        /// 在当前线程上开始运行标准应用程序消息循环 
        /// </summary>
        /// <typeparam name="T">窗体类型，必须实现<see cref="IRoleProvider"/>接口且必须有new()构造函数</typeparam>
        /// <remarks>
        /// 此方法作用实现了Application.Run功能之外，还实现了下列功能
        /// <para>1.设置未处理异常处理函数</para>
        /// <para>2.从配置文件winForm.config中加载配置</para>
        /// <para>3.显示设置的初始化窗体，并在系统启动之后自动关闭</para>
        /// <para>4.实现用户权限管理</para>
        /// </remarks>
        public static void Run<T>()
            where T : Form, IHost, new()
        {
            SetUnhandledException();
            using (var loader = new WinFormConfigXmlLoader())
            {
                SetAppConfig(loader.Load("winform.config"));
            }
            //if (appConfig.splashFormType != null)
            //{
            //    var splashForm = (Form)Activator.CreateInstance(appConfig.splashFormType);
            //    ThreadPool.QueueUserWorkItem(s =>
            //    {
            //        splashForm.StartPosition = FormStartPosition.CenterScreen;
            //        splashForm.ShowDialog();
            //    });
            //}
            var mainForm = new T();
            runningContext = mainForm.RunningContext;
            SetMenu(mainForm);
            Application.Run(mainForm);
        }
        static WinFormAppConfig appConfig = null;
        static RunningContext runningContext;
        static void SetAppConfig(WinFormAppConfig config)
        {
            if (appConfig != null)
            {
                appConfig.Dispose();
                appConfig = null;
            }
            appConfig = config;
        }
        static void SetUnhandledException() //设置未处理异常处理函数
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //为UI线程添加非捕获异常处理函数
            Application.ThreadException += (s, e) =>
            {
                try
                {
                    if (e.Exception is System.Security.SecurityException)  //对SecurityException做特殊处理，这个异常通常是因为权限不足
                    {
                        MessageBox.Show(e.Exception.Message, "权限不足");
                    }
                    else
                    {
                        var errorMsg = string.Format("{0}\n\nStack Trace:\n{1}", e.Exception.Message, e.Exception.StackTrace);
                        MessageBox.Show(errorMsg, "未捕获异常", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        MessageBox.Show("UI线程非捕获异常处理函数发生错误\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    finally
                    {
                        Application.Exit();
                    }
                }
            };
            //为非UI线程添加非捕获异常处理函数
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    var ex = (Exception)e.ExceptionObject;
                    if (ex is System.Security.SecurityException) //对SecurityException做特殊处理，这个异常通常是因为权限不足
                    {
                        MessageBox.Show(ex.Message, "权限不足");
                    }
                    else
                    {
                        if (!EventLog.SourceExists("ImmtAppException"))   //这里不能阻止程序的退出，因此将错误记录进系统日志
                        {
                            EventLog.CreateEventSource("ImmtAppException", "");
                        }
                        new EventLog { Source = "ImmtAppException" }.WriteEntry(string.Format("{0}\n\nStack Trace:\n{1}", ex.Message, ex.StackTrace));
                    }
                }
                catch (Exception exc)
                {
                    try
                    {
                        MessageBox.Show("无法将错误写入错误日志\n" + exc.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    finally
                    {
                        Application.Exit();
                    }
                }
            };
        }
        static void SetMenu(Form form)
        {
            if (appConfig.menuConfig == null || appConfig.menuConfig.Count == 0)
                return;

            var menuStrip = new MenuStrip();
            menuStrip.SuspendLayout();
            form.SuspendLayout();

            foreach (var itemConfig in appConfig.menuConfig)
            {
                var menuItem = new ToolStripMenuItem();
                SetMenuItem(menuItem, itemConfig);
                //if (itemConfig.ContainRoles(runningContext.CurrentUser.Roles))
                //{
                //    if (menuItem.DropDownItems.Count > 0)
                //        menuStrip.Items.Add(menuItem);
                //    else
                //        menuItem.Dispose();
                //}
                //else
                //{
                //    menuItem.Enabled = false;
                //    if (appConfig.InvalidMenuDisplayMode == InvalidMenuDisplayMode.Disable)
                //        menuStrip.Items.Add(menuItem);
                //    else
                //        menuItem.Dispose();
                //}
            }

            form.Controls.Add(menuStrip);
            form.MainMenuStrip = menuStrip;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            form.ResumeLayout(false);
            form.PerformLayout();
        }
        static void SetMenuItem(ToolStripMenuItem menuItem, MenuItemConfig menuItemConfig)
        {
            menuItem.Text = menuItemConfig.menuItemBase.Text;
            menuItem.Click += (s, e) =>
            {
                menuItemConfig.menuItemBase.OnClick(s as Form, runningContext);
            };
            foreach (var itemConfig in menuItemConfig.childrenConfig)
            {
                var childItem = new ToolStripMenuItem();
                SetMenuItem(childItem, itemConfig);
                //if (itemConfig.ContainRoles(runningContext.CurrentUser.Roles))
                //{
                //    if (childItem.DropDownItems.Count > 0)
                //        menuItem.DropDownItems.Add(childItem);
                //    else
                //        childItem.Dispose();
                //}
                //else
                //{
                //    menuItem.Enabled = false;
                //    if (appConfig.InvalidMenuDisplayMode == InvalidMenuDisplayMode.Disable)
                //        menuItem.DropDownItems.Add(childItem);
                //    else
                //        childItem.Dispose();
                //}
            }
        }
    }
}
