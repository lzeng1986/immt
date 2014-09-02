using System.ComponentModel;

namespace LazyBones.UI.Controls
{
    public class DropDownRichTextBox : DropDownControl
    {
        private System.Windows.Forms.RichTextBox richTextBox;

        public DropDownRichTextBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox.Location = new System.Drawing.Point(3, 24);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(200, 150);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            // 
            // DropDownRichTextBox
            // 
            this.Controls.Add(this.richTextBox);
            this.DropDownItem = this.richTextBox;
            this.Name = "DropDownRichTextBox";
            this.Size = new System.Drawing.Size(232, 212);
            this.ResumeLayout(false);

        }
        public override string Text
        {
            get { return richTextBox.Text; }
            set
            {
                richTextBox.Text = value;
            }
        }
    }
}
