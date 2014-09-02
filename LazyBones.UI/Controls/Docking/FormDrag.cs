using System;
using System.Windows.Forms;
using LazyBones.Win32;

namespace LazyBones.UI.Controls.Docking
{
    internal class FormDrag : Form
    {
        public FormDrag()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            SetStyle(ControlStyles.Selectable, false);
            Enabled = false;
            TopMost = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (int)(WindowExStyles.WS_EX_NOACTIVATE | WindowExStyles.WS_EX_TOOLWINDOW);
                return createParams;
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WinMsg.WM_NCHITTEST)
            {
                m.Result = (IntPtr)HitTest.Transparent;
                return;
            }

            base.WndProc(ref m);
        }
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        public virtual void ShowAndActivate()
        {
            Show();
            Activate();
        }
    }
}
