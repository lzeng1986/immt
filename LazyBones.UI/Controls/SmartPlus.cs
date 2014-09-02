using System;
using System.ComponentModel;

using LazyBones.Win32;

namespace LazyBones.UI.Controls
{
    /// <summary>
    /// 为提供一些针对Form的增强功能
    /// </summary>
    public partial class SmartPlus : ComponentWithContainerForm
    {
        public SmartPlus()
        {
            InitializeComponent();
        }

        public SmartPlus(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        void CheckForm()
        {
            if (Form == null)
                throw new InvalidOperationException("需设置Form属性");
        }
        public override System.Windows.Forms.Form Form
        {
            get
            {
                return base.Form;
            }
            set
            {
                base.Form = value;
                if (DesignMode)
                    return;
                fullScreen = new FullScreen(value);
                formHost.KeyDown += formHost_KeyDown;
            }
        }

        void formHost_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
                FullScreen = false;
        }
        FullScreen fullScreen;
        [Category("SmartPlus"),Description("设置窗体是否全屏显示"),DefaultValue(false)]
        public bool FullScreen
        {
            get
            {
                if(fullScreen == null)
                    return false;
                return fullScreen.IsFullScreen;
            }
            set
            {
                if (fullScreen == null)
                    return;
                fullScreen.IsFullScreen = value;
            }
        }
        Speed speed = Speed.Normal;
        [Category("SmartPlus"), Description("设置窗体动画速度"), DefaultValue(typeof(Speed), "Normal")]
        public Speed AcionSpeed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }
        [Category("SmartPlus"), Description("设置窗体动画的动作"), DefaultValue(typeof(Acions), "FeedIn")]
        public Acions Acions { get; set; }
        public void DoAction()
        {
            CheckForm();
            switch (Acions)
            {
                case Acions.FeedIn:
                    DoAnimation(new FadeIn(), speed);
                    break;
                case Acions.FeedOut:
                    DoAnimation(new FadeOut(), speed);
                    break;
                case Acions.SlideDown:
                    DoAnimation(new SlideDown(), speed);
                    break;
            }
        }
        IAnimation lastAnimation = null;
        void StopAnimation()
        {
            if (lastAnimation != null)
            {
                lastAnimation.End(Form);
                lastAnimation.Finished -= lastAnimation_Finished;
            }
        }
        void DoAnimation(IAnimation animation, Speed speed)
        {
            StopAnimation();
            lastAnimation = animation;
            lastAnimation.Finished += lastAnimation_Finished;
            lastAnimation.Init(Form, (int)speed / animationTimer.Interval);
            animationTimer.Start();
        }
        void lastAnimation_Finished(object sender, EventArgs e)
        {
            animationTimer.Stop();
        }
        private void animationTimer_Tick(object sender, EventArgs e)
        {
            if (lastAnimation != null)
            {
                lastAnimation.Step(Form);
            }
        }

        int maxIdleSeconds = 60;
        [Category("SmartPlus"), Description("系统最长空闲时间"), DefaultValue(60)]
        public int MaxIdleSeconds
        {
            get
            {
                return maxIdleSeconds;
            }
            set
            {
                if (value < 10)
                    throw new ArgumentOutOfRangeException("空闲时间不得小于10秒");
                maxIdleSeconds = value;
            }
        }
        [Category("SmartPlus"), Description("锁定的方式"), DefaultValue(typeof(LockMode), "LockForm")]
        public LockMode LockMode { get; set; }

        public void StartLock()
        {
            lockTimer.Start();
        }
        public void StopLock()
        {
            lockTimer.Stop();
        }

        private void lockTimer_Tick(object sender, EventArgs e)
        {
            var inputInfo = new LastInputInfo();
            inputInfo.Size = unchecked((uint)System.Runtime.InteropServices.Marshal.SizeOf(inputInfo));
            User32.GetLastInputInfo(ref inputInfo);
            var now = Kernel32.GetTickCount64();
            if (now - inputInfo.Time >= (ulong)maxIdleSeconds * 1000L)
            {
                switch (LockMode)
                {
                    case LockMode.LockForm:
                        Form.Enabled = false;
                        break;
                    case LockMode.LockSystem:
                        User32.LockWorkStation();
                        break;
                }
            }
        }
        void CreateFlashWInfo(ref FlashWInfo info)
        {
            info.Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.Timeout = 0;
        }
        public void FlashForm()
        {
            FlashForm(uint.MaxValue);
        }
        public void FlashForm(uint flashTimes)
        {
            CheckForm();
            CheckOSVersion();
            FlashWInfo flashInfo = new FlashWInfo();
            CreateFlashWInfo(ref flashInfo);
            flashInfo.Handle = Form.Handle;
            flashInfo.Flags = FlashFlag.Tray;
            flashInfo.Count = flashTimes;
            User32.FlashWindowEx(ref flashInfo);
        }
        /// <summary>
        /// 停止闪烁窗体
        /// </summary>
        public void StopFlashForm()
        {
            CheckForm();
            CheckOSVersion();
            FlashWInfo flashInfo = new FlashWInfo();
            CreateFlashWInfo(ref flashInfo);
            flashInfo.Handle = Form.Handle;
            flashInfo.Flags = FlashFlag.Stop;
            User32.FlashWindowEx(ref flashInfo);
        }
        /// <summary>
        /// 闪烁窗体直到窗体被激活
        /// </summary>
        public void FlashFormUntilActived()
        {
            CheckForm();
            CheckOSVersion();
            FlashWInfo flashInfo = new FlashWInfo();
            CreateFlashWInfo(ref flashInfo);
            flashInfo.Handle = Form.Handle;
            flashInfo.Flags = FlashFlag.Tray | FlashFlag.TimerNoForeground;
            User32.FlashWindowEx(ref flashInfo);
        }
        void CheckOSVersion()
        {
            if (!Native.IsWin2KOrLater)
            {
                throw new PlatformNotSupportedException("需要Win2000或更高版本");
            }
        }
    }
    /// <summary>
    /// 表示锁定的方式
    /// </summary>
    [Description("锁定方式")]
    public enum LockMode
    {
        /// <summary>
        /// 锁定窗体
        /// </summary>
        [Description("锁定窗体")]
        LockForm = 0,
        /// <summary>
        /// 锁定系统
        /// </summary>
        [Description("锁定系统")]
        LockSystem
    }
}
