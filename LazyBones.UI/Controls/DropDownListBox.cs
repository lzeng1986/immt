using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace LazyBones.UI.Controls
{
    public class DropDownListBox : DropDownControl
    {
        private System.Windows.Forms.ListBox listBox;

        public DropDownListBox()
        {
            InitializeComponent();
            listBox.Width = this.Width;
        }

        private void InitializeComponent()
        {
            this.listBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listBox
            // 
            this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBox.FormattingEnabled = true;
            this.listBox.IntegralHeight = false;
            this.listBox.ItemHeight = 12;
            this.listBox.Location = new System.Drawing.Point(3, 23);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(144, 117);
            this.listBox.TabIndex = 0;
            this.listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // DropDownListBox
            // 
            this.Controls.Add(this.listBox);
            this.DropDownItem = this.listBox;
            this.Name = "DropDownListBox";
            this.ResumeLayout(false);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            listBox.Width = this.Width;
        }

        [DefaultValue(""), Category("Data")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get { return listBox.DataSource; }
            set { listBox.DataSource = value; }
        }

        [DefaultValue(""), Category("Data")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get { return listBox.DisplayMember; }
            set { listBox.DisplayMember = value; }
        }

        [DefaultValue(""), Category("Data")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get { return listBox.ValueMember; }
            set { listBox.ValueMember = value; }
        }

        public override string Text
        {
            get { return listBox.Text; }
            set
            {
                listBox.Text = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public int SelectedIndex { get; set; }

        [Browsable(false)]
        public object SelectedValue{ get; private set; }

        int maxDropDownItems = 9;
        [Category("DropDown"), DefaultValue(9)]
        public int MaxDropDownItems
        {
            get { return maxDropDownItems; }
            set { maxDropDownItems = value; }
        }
        protected override void OnDropDownOpening()
        {
            var dropDownCount = Math.Min(maxDropDownItems, listBox.Items.Count);
            listBox.Height = listBox.Items.Count * listBox.ItemHeight + 10;
            base.OnDropDownOpening();
        }
        protected override void OnDropDownOpened()
        {
            listBox.SelectedIndex = SelectedIndex;
            base.OnDropDownOpened();
        }
        protected override void OnDropDownClosed()
        {
            base.OnDropDownClosed();
            SelectedValue = listBox.SelectedValue;
        }
        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            base.Text = listBox.Text;
            SelectedIndex = listBox.SelectedIndex;
        }
    }
}
