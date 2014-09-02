using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace LazyBones.UI.Controls
{
    [DefaultEvent("PopupContentClick")]
    [DefaultProperty("TrayVisible")]
    public class TrayNotifier : Component
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private IContainer components;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
        }

        public TrayNotifier()
        {
            InitializeComponent();
        }

        public TrayNotifier(IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");
            container.Add(this);
            InitializeComponent();
        }

        [Category("Tray")]
        [DefaultValue("")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string TrayText
        {
            get { return notifyIcon.Text; }
            set { notifyIcon.Text = value; }
        }
        bool trayVisible = false;
        [Category("Tray"), DefaultValue(false)]
        public bool TrayVisible
        {
            get { return trayVisible; }
            set
            {
                trayVisible = value;
                if (DesignMode)
                    return;
                notifyIcon.Visible = value;
            }
        }
        [Category("Tray")]
        public Icon TrayIcon
        {
            get { return notifyIcon.Icon; }
            set { notifyIcon.Icon = value; }
        }
        [Category("Tray")]
        public ContextMenuStrip TrayContextMenuStrip
        {
            get { return notifyIcon.ContextMenuStrip; }
            set { notifyIcon.ContextMenuStrip = value; }
        }
        [Category("Tray")]
        public event MouseEventHandler TrayMouseClick
        {
            add { notifyIcon.MouseClick += value; }
            remove { notifyIcon.MouseClick -= value; }
        }
        [Category("Tray")]
        public event MouseEventHandler TrayMouseDoubleClick
        {
            add { notifyIcon.MouseDoubleClick += value; }
            remove { notifyIcon.MouseDoubleClick -= value; }
        }
        [Category("Tray")]
        public event EventHandler TrayBalloonTipClicked
        {
            add { notifyIcon.BalloonTipClicked += value; }
            remove { notifyIcon.BalloonTipClicked -= value; }
        }

        Color popupHeaderColor = SystemColors.ControlDark;
        [Category("Popup"), DefaultValue(typeof(Color), "ControlDark")]
        public Color PopupHeaderColor
        {
            get { return popupHeaderColor; }
            set { popupHeaderColor = value; }
        }

        int popupHeaderHeight = 10;
        [Category("Popup"), DefaultValue(10)]
        public int PopupHeaderHeight
        {
            get { return popupHeaderHeight; }
            set { popupHeaderHeight = value; }
        }

        Color popupBodyColor = SystemColors.Control;
        [Category("Popup"), DefaultValue(typeof(Color), "Control")]
        public Color PopupBodyColor
        {
            get { return popupBodyColor; }
            set { popupBodyColor = value; }
        }

        int gradientPower = 50;
        [Category("Popup"), DefaultValue(50)]
        public int GradientPower
        {
            get { return gradientPower; }
            set { gradientPower = value; }
        }

        Size popupImageSize = new Size(32, 32);
        [Category("Popup"), DefaultValue(typeof(Size), "32,32")]
        public Size PopupImageSize
        {
            get { return PopupImage == null ? Size.Empty : popupImageSize; }
            set { popupImageSize = value; }
        }

        Padding popupImageMargin = new Padding(5);
        [Category("Popup"), DefaultValue(typeof(Padding), "5,5,5,5")]
        public Padding PopupImageMargin
        {
            get { return PopupImage == null ? Padding.Empty : popupImageMargin; }
            set
            {
                if (PopupImage == null)
                    return;
                popupImageMargin = value;
            }
        }

        [Category("Popup")]
        public Image PopupImage { get; set; }

        [Category("Popup"), DefaultValue(PopupMode.Slide)]
        public PopupMode PopupMode { get; set; }

        Color popupBorderColor = SystemColors.WindowFrame;
        [Category("Popup"), DefaultValue(typeof(Color), "WindowFrame")]
        public Color PopupBorderColor
        {
            get { return popupBorderColor; }
            set { popupBorderColor = value; }
        }

        Size popupWindowSize = new Size(300, 100);
        [Category("Popup"), DefaultValue(typeof(Size), "300,100")]
        public Size PopupWindowSize
        {
            get { return popupWindowSize; }
            set { popupWindowSize = value; }
        }

        int popupDuration = 1000;
        [Category("Popup"), DefaultValue(1000)]
        public int PopupDuration
        {
            get { return popupDuration; }
            set { popupDuration = value; }
        }

        int popupTimeOut = 3000;
        [Category("Popup"), DefaultValue(3000)]
        public int PopupTimeOut
        {
            get { return popupTimeOut; }
            set { popupTimeOut = value; }
        }

        Color popupTitleColor = Color.Gray;
        [Category("PopupTitle"), DefaultValue(typeof(Color), "Gray")]
        public Color PopupTitleColor
        {
            get { return popupTitleColor; }
            set { popupTitleColor = value; }
        }

        Padding popupTitleMargin = new Padding(3);
        [Category("PopupTitle"), DefaultValue(typeof(Padding), "3,3,3,3")]
        public Padding PopupTitleMargin
        {
            get { return popupTitleMargin; }
            set { popupTitleMargin = value; }
        }

        Font popupTitleFont = SystemFonts.CaptionFont;
        [Category("PopupTitle"), DefaultValue(typeof(Font), "CaptionFont")]
        public Font PopupTitleFont
        {
            get { return popupTitleFont; }
            set { popupTitleFont = value; }
        }

        [Category("PopupTitle"), DefaultValue("")]
        public string PopupTitleText { get; set; }

        Color popupContentColor = SystemColors.ControlText;
        [Category("PopupContent"), DefaultValue(typeof(Color), "ControlText")]
        public Color PopupContentColor
        {
            get { return popupContentColor; }
            set { popupContentColor = value; }
        }

        Color popupContentHoverColor = SystemColors.HotTrack;
        [Category("PopupContent"), DefaultValue(typeof(Color), "HotTrack")]
        public Color PopupContentHoverColor
        {
            get { return popupContentHoverColor; }
            set { popupContentHoverColor = value; }
        }

        Font popupContentFont = SystemFonts.DialogFont;
        [Category("PopupContent"), DefaultValue(typeof(Font), "DialogFont")]
        public Font PopupContentFont
        {
            get { return popupContentFont; }
            set { popupContentFont = value; }
        }

        Padding popupContentMargin = new Padding(3);
        [Category("PopupContent"), DefaultValue(typeof(Padding), "3,3,3,3")]
        public Padding PopupContentMargin
        {
            get { return popupContentMargin; }
            set { popupContentMargin = value; }
        }

        Color popupButtonBorderColor = SystemColors.WindowFrame;
        [Category("PopupButton"), DefaultValue(typeof(Color), "WindowFrame")]
        public Color PopupButtonBorderColor
        {
            get { return popupButtonBorderColor; }
            set { popupButtonBorderColor = value; }
        }

        Color popupButtonHoverColor = SystemColors.Highlight;
        [Category("PopupButton"), DefaultValue(typeof(Color), "Highlight")]
        public Color PopupButtonHoverColor
        {
            get { return popupButtonHoverColor; }
            set { popupButtonHoverColor = value; }
        }

        Size popupButtonSize = new Size(16, 16);
        [Category("PopupButton"), DefaultValue(typeof(Size), "16,16")]
        public Size PopupButtonSize
        {
            get { return popupButtonSize; }
            set { popupButtonSize = value; }
        }

        Padding popupButtonMargin = new Padding(3);
        [Category("PopupButton"), DefaultValue(typeof(Padding), "3,3,3,3")]
        public Padding PopupButtonMargin
        {
            get { return popupButtonMargin; }
            set { popupButtonMargin = value; }
        }

        [Category("Popup")]
        public event EventHandler PopupContentClick;

        public void ShowBalloonTip(int timeout, string title, string text, ToolTipIcon icon)
        {
            notifyIcon.ShowBalloonTip(timeout, title, text, icon);
        }
        public void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            ShowBalloonTip(2000, title, text, icon);
        }
        public void ShowBalloonTip(string title, string text)
        {
            ShowBalloonTip(2000, title, text, ToolTipIcon.Info);
        }
        public void Popup(string text)
        {
            Popup(PopupTitleText, text, PopupTimeOut, PopupDuration);
        }
        public void Popup(string text, Action textClickAction)
        {
            Popup(PopupTitleText, text, PopupTimeOut, PopupDuration, textClickAction);
        }
        public void Popup<T>(string text, Action<T> textClickAction, T arg)
        {
            Popup(PopupTitleText, text, PopupTimeOut, PopupDuration, () => textClickAction(arg));
        }
        public void Popup(string title, string text)
        {
            Popup(title, text, PopupTimeOut, PopupDuration);
        }
        public void Popup(string title, string text, Action textClickAction)
        {
            Popup(title, text, PopupTimeOut, PopupDuration, textClickAction);
        }
        public void Popup<T>(string title, string text, Action<T> textClickAction, T arg)
        {
            Popup(title, text, PopupTimeOut, PopupDuration, () => textClickAction(arg));
        }
        public void Popup(string title, string text, int timeout, int duration)
        {
            Popup(title, text, timeout, duration, null);
        }
        public void Popup<T>(string title, string text, int timeout, int duration, Action<T> textClickAction, T arg)
        {
            Popup(title, text, timeout, duration, () => textClickAction(arg));
        }
        public void Popup(string title, string text, int timeout, int duration, Action textClickAction)
        {
            var behaviour = new PopupBehaviour
            {
                Title = title,
                Text = text,
                Timeout = timeout,
                Duration = duration
            };

            var form = new TrayNotifyWindow(this, behaviour);
            form.ContentClick += form_ContentClick;
            form.Tag = textClickAction;
            form.Show();
        }

        void form_ContentClick(object sender, EventArgs e)
        {
            var form = sender as TrayNotifyWindow;
            form.ContentClick += form_ContentClick;
            var action = form.Tag as Action;
            if (action != null)
            {
                action();
                form.Tag = null;
            }
            else
            {
                if (PopupContentClick != null)
                    PopupContentClick(this, EventArgs.Empty);
            }
        }

        //class NotifierAction : IDisposable
        //{
        //    IContainer components = new Container();
        //    Timer timerAnimation;
        //    Timer timerWait;
        //    Form notiferWindow;

        //    double yStart;
        //    double yStop;
        //    double yInterval;
        //    double opacityStart;
        //    double opacityStop;
        //    double opacityInterval;

        //    private NotifierAction(PopupBehaviour behaviour, Form notiferWindow)
        //    {
        //        timerAnimation = new Timer(components) { Interval = 10 };
        //        timerAnimation.Tick += timerAnimation_Tick;
        //        timerWait = new Timer(components) { Interval = behaviour.Timeout };
        //        timerWait.Tick += timerWait_Tick;
        //        notiferWindow.FormClosed += formNotifer_FormClosed;
        //        this.notiferWindow = notiferWindow;
        //        var screen = Screen.FromControl(notiferWindow);
        //        notiferWindow.Top = screen.Bounds.Bottom + behaviour.WindowSize.Height;
        //        notiferWindow.Left = screen.WorkingArea.Right - behaviour.WindowSize.Width;
        //        switch (behaviour.PopupMode)
        //        {
        //            case PopupMode.Fade:
        //                yStart = screen.WorkingArea.Bottom - behaviour.WindowSize.Height;
        //                yStop = yStart;
        //                break;
        //            case PopupMode.Slide:
        //                yStart = screen.Bounds.Bottom;
        //                yStop = screen.WorkingArea.Bottom - behaviour.WindowSize.Height;
        //                break;
        //        }
        //        opacityStart = 0.0;
        //        opacityStop = 1.0;
        //        yInterval = (yStop - yStart) * timerAnimation.Interval / behaviour.Duration;
        //        opacityInterval = (opacityStop - opacityStart) * timerAnimation.Interval / behaviour.Duration;
        //        timerWait.Interval = behaviour.Timeout;
        //        timerAnimation.Start();
        //    }

        //    void formNotifer_FormClosed(object sender, FormClosedEventArgs e)
        //    {

        //        notiferWindow.FormClosed -= formNotifer_FormClosed;
        //        notiferWindow = null;
        //    }

        //    void timerAnimation_Tick(object sender, EventArgs e)
        //    {
        //        opacityStart += opacityInterval;
        //        yStart += yInterval;
        //        if (opacityStart >= opacityStop && yStart <= yStop)
        //        {
        //            timerAnimation.Stop();
        //            notiferWindow.Opacity = opacityStop;
        //            notiferWindow.Top = (int)yStop;
        //            timerWait.Start();
        //        }
        //        else
        //        {
        //            notiferWindow.Opacity = Math.Min(opacityStart, 1.0);
        //            notiferWindow.Top = (int)yStart;
        //        }
        //    }

        //    void timerWait_Tick(object sender, EventArgs e)
        //    {
        //        notiferWindow.Close();
        //    }
        //    public void Do() {
        //        notiferWindow.Show();
        //    }
        //    public static void Open(PopupBehaviour behaviour, Form notiferWindow)
        //    {
        //        notiferWindow.FormBorderStyle = FormBorderStyle.None;   //修改窗体必要属性
        //        notiferWindow.ShowInTaskbar = false;
        //        notiferWindow.TopMost = true;
        //        var action = new NotifierAction(behaviour, notiferWindow);
        //        action.Do();
        //    }
        //    public void Dispose()
        //    {
        //        components.Dispose();
        //    }
        //}
    }

    public enum PopupMode
    {
        Slide,
        Fade
    }

    struct PopupBehaviour
    {
        public string Title;
        public string Text;
        public int Timeout;
        public int Duration;
        public PopupMode PopupMode;
        public Size WindowSize;
    }
}
