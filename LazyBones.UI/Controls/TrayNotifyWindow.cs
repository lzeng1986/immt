using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LazyBones.UI.Controls
{
    class TrayNotifyWindow : Form
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerAnimation = new System.Windows.Forms.Timer(this.components);
            this.timerWait = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // timerAnimation
            // 
            this.timerAnimation.Interval = 10;
            this.timerAnimation.Tick += new System.EventHandler(this.timerAnimation_Tick);
            // 
            // timerWait
            // 
            this.timerWait.Tick += new System.EventHandler(this.timerWait_Tick);
            // 
            // toolTip
            // 
            this.toolTip.Active = false;
            // 
            // FormNotifyWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormNotifyWindow";
            this.ShowInTaskbar = false;
            this.Text = "FormNotifyWindow";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerAnimation;
        private System.Windows.Forms.Timer timerWait;
        private System.Windows.Forms.ToolTip toolTip;

        public event EventHandler ContentClick;
        public event EventHandler CloseButtonClick;

        bool mouseOnClose = false;
        bool mouseOnContent = false;
        bool contentNeedTooltip = false;
        int titleHeigth;

        double yStart;
        double yStop;
        double yInterval;
        double opacityStart;
        double opacityStop;
        double opacityInterval;

        Rectangle rectHeader;
        Rectangle rectForm;
        Rectangle rectCloseButton;
        Rectangle rectTitle;
        Rectangle rectContent;
        Rectangle rectImage;
        Rectangle rectContentProposed;
        LinearGradientBrush brushBody;
        LinearGradientBrush brushHeader;
        Brush brushButtonHover;
        Pen penButtonBorder;
        Pen penBorder;
        Font fontContentHover;

        TrayNotifier parent;
        PopupBehaviour behaviour;

        public TrayNotifyWindow(TrayNotifier parent, PopupBehaviour behaviour)
        {
            InitializeComponent();
            this.parent = parent;
            this.behaviour = behaviour;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer 
                | ControlStyles.ResizeRedraw 
                | ControlStyles.AllPaintingInWmPaint, true);
        }

        void InitializeObjects()
        {
            rectForm = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            rectHeader = new Rectangle(0, 0, this.Width, parent.PopupHeaderHeight);

            rectCloseButton = new Rectangle(
                    Width - parent.PopupButtonMargin.Horizontal - parent.PopupButtonSize.Width,
                    rectHeader.Bottom + parent.PopupButtonMargin.Top,
                    parent.PopupButtonSize.Width,
                    parent.PopupButtonSize.Height);

            rectImage = new Rectangle(
                parent.PopupImageMargin.Left,
                rectHeader.Bottom + parent.PopupImageMargin.Top,
                parent.PopupImageSize.Width,
                parent.PopupImageSize.Height);

            titleHeigth = TextRenderer.MeasureText(behaviour.Title, parent.PopupTitleFont).Height;
            rectTitle = new Rectangle(
                rectImage.Right + parent.PopupTitleMargin.Left,
                rectHeader.Bottom + parent.PopupTitleMargin.Top,
                Width - rectImage.Width - parent.PopupTitleMargin.Horizontal - rectCloseButton.Width - parent.PopupButtonMargin.Horizontal,
                titleHeigth);

            rectContent = new Rectangle(
                    parent.PopupImageMargin.Horizontal + parent.PopupImageSize.Width + parent.PopupContentMargin.Left,
                    parent.PopupHeaderHeight + titleHeigth + parent.PopupTitleMargin.Vertical,
                    Width - parent.PopupImageMargin.Horizontal - parent.PopupImageSize.Width - parent.PopupContentMargin.Horizontal,
                    Height - parent.PopupHeaderHeight - titleHeigth - parent.PopupTitleMargin.Vertical - parent.PopupContentMargin.Vertical
                    );

            brushBody = new LinearGradientBrush(ClientRectangle, parent.PopupBodyColor, GetLighterColor(parent.PopupBodyColor), LinearGradientMode.Vertical);
            brushHeader = new LinearGradientBrush(rectHeader, parent.PopupHeaderColor, GetDarkerColor(parent.PopupHeaderColor), LinearGradientMode.Vertical);
            brushButtonHover = new SolidBrush(parent.PopupButtonHoverColor);
            penButtonBorder = new Pen(parent.PopupButtonBorderColor);
            penBorder = new Pen(parent.PopupBorderColor);
            fontContentHover = new Font(parent.PopupContentFont, FontStyle.Underline);
        }
        void DisposeObjects()
        {
            brushBody.Dispose();
            brushHeader.Dispose();
            brushButtonHover.Dispose();
            penButtonBorder.Dispose();
            penBorder.Dispose();
            fontContentHover.Dispose();
        }

        int AddValueMax255(int input, int add)
        {
            return Math.Min(input + add, 255);
        }

        int DedValueMin0(int input, int ded)
        {
            return Math.Max(input - ded, 0);
        }

        Color GetDarkerColor(Color color)
        {
            return Color.FromArgb(255, DedValueMin0((int)color.R, parent.GradientPower), DedValueMin0((int)color.G, parent.GradientPower), DedValueMin0((int)color.B, parent.GradientPower));
        }

        Color GetLighterColor(Color color)
        {
            return Color.FromArgb(255, AddValueMax255((int)color.R, parent.GradientPower), AddValueMax255((int)color.G, parent.GradientPower), AddValueMax255((int)color.B, parent.GradientPower));
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible)
            {
                mouseOnClose = mouseOnContent = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Size = parent.PopupWindowSize;
            InitializeObjects();
            var screen = Screen.FromControl(this);
            Top = screen.Bounds.Bottom + Size.Height;
            Left = screen.WorkingArea.Right - Size.Width;
            switch (parent.PopupMode)
            {
                case PopupMode.Fade:
                    yStart = screen.WorkingArea.Bottom - Size.Height;
                    yStop = yStart;
                    break;
                case PopupMode.Slide:
                    yStart = screen.Bounds.Bottom;
                    yStop = screen.WorkingArea.Bottom - Size.Height;
                    break;
            }
            opacityStart = 0.0;
            opacityStop = 1.0;
            yInterval = (yStop - yStart) * timerAnimation.Interval / behaviour.Duration;
            opacityInterval = (opacityStop - opacityStart) * timerAnimation.Interval / behaviour.Duration;
            timerWait.Interval = behaviour.Timeout;
            timerAnimation.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            DisposeObjects();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mouseOnContent = rectContent.Contains(e.Location);
            if (mouseOnContent)
            {
                if (contentNeedTooltip && !toolTip.Active)
                {
                    toolTip.Active = true;
                    toolTip.Show(behaviour.Text, this, rectContentProposed.Left, rectContentProposed.Bottom);
                }
            }
            else
            {
                toolTip.Active = false;
            }
            mouseOnClose = rectCloseButton.Contains(e.Location);
            Cursor = mouseOnContent ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
                return;
            if (mouseOnClose)
                OnCloseClick();
            if (mouseOnContent)
                OnContentClick();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangle(brushBody, ClientRectangle);
            e.Graphics.FillRectangle(brushHeader, rectHeader);
            e.Graphics.DrawRectangle(penBorder, rectForm);

            if (mouseOnClose)
            {
                e.Graphics.FillRectangle(brushButtonHover, rectCloseButton);
                e.Graphics.DrawRectangle(penButtonBorder, rectCloseButton);
            }

            e.Graphics.DrawLine(Pens.Black, rectCloseButton.Left + 4, rectCloseButton.Top + 4, rectCloseButton.Right - 4, rectCloseButton.Bottom - 4);
            e.Graphics.DrawLine(Pens.Black, rectCloseButton.Left + 4, rectCloseButton.Bottom - 4, rectCloseButton.Right - 4, rectCloseButton.Top + 4);

            if (parent.PopupImage != null)
            {
                e.Graphics.DrawImage(parent.PopupImage, rectImage);
            }

            TextRenderer.DrawText(e.Graphics, behaviour.Title, parent.PopupTitleFont, rectTitle, parent.PopupTitleColor, TextFormatFlags.Default);

            var contentColor = mouseOnContent ? parent.PopupButtonHoverColor : parent.PopupContentColor;
            var contentFont = mouseOnContent ? fontContentHover : parent.PopupContentFont;
            var contentDrawProposedSize = TextRenderer.MeasureText(behaviour.Text, contentFont);
            contentNeedTooltip = !rectContent.Contains(rectContent.Location + contentDrawProposedSize);
            rectContentProposed = new Rectangle(rectContent.Location, contentDrawProposedSize);
            TextRenderer.DrawText(e.Graphics, behaviour.Text, contentFont, rectContent, contentColor, TextFormatFlags.WordEllipsis);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (timerAnimation.Enabled)
                return;
            timerWait.Stop();
        }
        void OnCloseClick()
        {
            var handle = CloseButtonClick;
            if (handle != null)
                handle(this, EventArgs.Empty);
            Close();
        }
        void OnContentClick()
        {
            var handle = ContentClick;
            if (handle != null)
                handle(this, EventArgs.Empty);
            Close();
        }

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            opacityStart += opacityInterval;
            yStart += yInterval;
            if (opacityStart >= opacityStop && yStart <= yStop)
            {
                timerAnimation.Stop();
                this.Opacity = opacityStop;
                this.Top = (int)yStop;
                timerWait.Start();
            }
            else
            {
                this.Opacity = Math.Min(opacityStart, 1.0);
                this.Top = (int)yStart;
            }
        }

        private void timerWait_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
