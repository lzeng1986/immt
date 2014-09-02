using System;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.TextEditor
{
    class FormToolTip : Form//用于显示ToolTip的窗体
    {
        string description = String.Empty;
        public string Description
        {
            get { return description; }
            set
            {
                if (description == value)
                    return;
                description = value;
                if (string.IsNullOrEmpty(value))
                    Hide();
                else
                    Show();
                Refresh();
            }
        }
        public bool HideOnClick { get; set; }

        public FormToolTip()
        {
            SetStyle(ControlStyles.Selectable, false);
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopLevel = false;
            Size = Size.Empty;
            base.CreateHandle();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                //AbstractCompletionWindow.AddShadowToWindow(p);
                return p;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (HideOnClick)
                Hide();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (string.IsNullOrEmpty(description))
                return;
            //TipPainterTools.DrawHelpTipFromCombinedDescription(this, pe.Graphics, Font, null, description);
        }

        protected override void OnPaintBackground(PaintEventArgs pe)
        {
            pe.Graphics.FillRectangle(SystemBrushes.Info, pe.ClipRectangle);
        }
    }
}
