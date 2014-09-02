using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LazyBones.UI.Controls
{
    public class DropDownControl : UserControl
    {
        FormDropDownContainer dropDownContainer = null;
        private ToolTip toolTipText;
        private IContainer components;

        int titleHeight = 20;
        [DefaultValue(20)]
        [Category("DropDown")]
        public int TitleHeight
        {
            get { return titleHeight; }
            set
            {
                titleHeight = value;
                Invalidate();
            }
        }

        int dropDownHeight = 100;
        [DefaultValue(100)]
        [Category("DropDown")]
        public int DropDownHeight
        {
            get { return dropDownHeight; }
            set
            {
                dropDownHeight = value;
                Invalidate();
            }
        }

        Control dropDownItem = null;
        [DefaultValue(null)]
        [Category("DropDown")]
        public Control DropDownItem
        {
            get { return dropDownItem; }
            set
            {
                if (ReferenceEquals(value, this))
                    throw new ArgumentException("不能选择自己");
                dropDownItem = value;
            }
        }

        [Browsable(true)]
        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;

        public DropDownControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTipText = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // DropDownControl
            // 
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Name = "DropDownControl";
            this.ResumeLayout(false);
        }
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (ReferenceEquals(e.Control, dropDownItem) && !removeControlBySelf)
                dropDownItem = null;
            removeControlBySelf = false;
        }
        bool removeControlBySelf = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                this.Height = titleHeight;
                if (dropDownItem != null)
                {
                    removeControlBySelf = true;
                    dropDownItem.Parent.Controls.Remove(dropDownItem);
                }
            }
        }
        Rectangle textBounds;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            textBounds = new Rectangle(2, 2, ClientSize.Width - 21, titleHeight - 4);
            Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (textBounds.Contains(e.Location))
            {
                if (needToolTip && !toolTipText.Active)
                {
                    toolTipText.Active = true;
                    toolTipText.Show(Text, this, e.Location);
                }
            }
            else
            {
                toolTipText.Active = false;
            }
        }
        bool mousePressed = false;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mousePressed = true;
            OpenDropDown();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mousePressed = false;
            Invalidate();
        }

        bool needToolTip = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rectDropDownButton = new Rectangle(ClientSize.Width - 19, 2, 18, titleHeight - 4);
            if (ComboBoxRenderer.IsSupported)
            {
                var state = (mousePressed || dropDownContainer != null) ? ComboBoxState.Pressed : ComboBoxState.Normal;
                ComboBoxRenderer.DrawTextBox(e.Graphics, new Rectangle(0, 0, ClientSize.Width, titleHeight), state);
                ComboBoxRenderer.DrawDropDownButton(e.Graphics, rectDropDownButton, state);
            }
            else
            {
                ControlPaint.DrawComboButton(e.Graphics, rectDropDownButton, (Enabled) ? ButtonState.Normal : ButtonState.Inactive);
            }
            var s = TextRenderer.MeasureText(Text, this.Font);
            needToolTip = textBounds.Width < s.Width || textBounds.Height < s.Height;
            TextRenderer.DrawText(e.Graphics, Text, this.Font, this.textBounds, this.ForeColor, TextFormatFlags.WordEllipsis);
        }
        bool dropDownOpened = false;
        public void OpenDropDown()
        {
            if (dropDownItem == null)
                throw new NotImplementedException("属性DropDownItem没有赋值");
            if (dropDownOpened) //如果当前下拉框已经打开，则不应该再次打开，立即返回
                return;
            OnDropDownOpening();
            dropDownContainer = new FormDropDownContainer(dropDownItem);
            dropDownContainer.FormClosed += dropDownContainer_FormClosed;
            var bounds = DropDownBounds;
            dropDownContainer.Location = bounds.Location;
            dropDownContainer.Show(ParentForm);
            ParentForm.Move += ParentForm_Move;
            offset = new Size(ParentForm.Left - dropDownContainer.Left, ParentForm.Top - dropDownContainer.Top);
            Invalidate();
            dropDownOpened = true;
            OnDropDownOpened();
        }
        Size offset;
        void ParentForm_Move(object sender, EventArgs e)
        {
            dropDownContainer.Location = ParentForm.Location - offset;
        }

        void dropDownContainer_FormClosed(object sender, FormClosedEventArgs e)
        {
            dropDownContainer.FormClosed -= dropDownContainer_FormClosed;
            dropDownContainer.Owner.Move -= ParentForm_Move;
            dropDownContainer = null;
            Invalidate();
            OnDropDownClosed();
            dropDownOpened = false;
        }
        protected virtual Rectangle DropDownBounds
        {
            get
            {
                if (dropDownItem == null)
                    return Rectangle.Empty;
                var inflatedDropSize = dropDownItem.Size + new Size(2, 2);
                return Parent.RectangleToScreen(new Rectangle(Left, Bottom, inflatedDropSize.Width, inflatedDropSize.Height));
            }
        }
        protected virtual void OnDropDownOpening()
        {
        }
        protected virtual void OnDropDownOpened()
        {
            var handler = DropDownOpened;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnDropDownClosed()
        {
            var handler = DropDownClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        class FormDropDownContainer : Form, IMessageFilter
        {
            public FormDropDownContainer(Control dropDownItem)
            {
                InitializeComponent();
                dropDownItem.Location = new Point(1, 1);
                Controls.Add(dropDownItem);
            }
            void InitializeComponent()
            {
                this.SuspendLayout();
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.ResumeLayout(false);
            }
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                Application.AddMessageFilter(this);
            }
            public bool PreFilterMessage(ref Message m)
            {
                if(this != Form.ActiveForm)
                    Close();
                return false;
            }
            protected override void OnFormClosed(FormClosedEventArgs e)
            {
                Application.RemoveMessageFilter(this);
                base.OnFormClosed(e);
                Controls.RemoveAt(0);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.DrawRectangle(Pens.Gray, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }
    }
}
