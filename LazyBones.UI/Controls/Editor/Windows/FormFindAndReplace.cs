using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Editor
{
    class FormFindAndReplace : Form
    {
        private Button btnReplaceAll;
        private Button btnReplace;
        private Button btnFindPrevious;
        private Button btnFindNext;
        private TextBox txtReplaceWith;
        private TextBox txtLookFor;
        private Label lblReplaceWith;
        private Label label1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox1;
    
        private void InitializeComponent()
        {
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnFindPrevious = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.txtReplaceWith = new System.Windows.Forms.TextBox();
            this.txtLookFor = new System.Windows.Forms.TextBox();
            this.lblReplaceWith = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Location = new System.Drawing.Point(263, 119);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(76, 21);
            this.btnReplaceAll.TabIndex = 21;
            this.btnReplaceAll.Text = "替换所有";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            // 
            // btnReplace
            // 
            this.btnReplace.Location = new System.Drawing.Point(180, 119);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(76, 21);
            this.btnReplace.TabIndex = 19;
            this.btnReplace.Text = "替换";
            this.btnReplace.UseVisualStyleBackColor = true;
            // 
            // btnFindPrevious
            // 
            this.btnFindPrevious.Location = new System.Drawing.Point(12, 119);
            this.btnFindPrevious.Name = "btnFindPrevious";
            this.btnFindPrevious.Size = new System.Drawing.Size(76, 21);
            this.btnFindPrevious.TabIndex = 17;
            this.btnFindPrevious.Text = "上一个";
            this.btnFindPrevious.UseVisualStyleBackColor = true;
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(97, 119);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(76, 21);
            this.btnFindNext.TabIndex = 18;
            this.btnFindNext.Text = "下一个";
            this.btnFindNext.UseVisualStyleBackColor = true;
            // 
            // txtReplaceWith
            // 
            this.txtReplaceWith.Location = new System.Drawing.Point(12, 65);
            this.txtReplaceWith.Name = "txtReplaceWith";
            this.txtReplaceWith.Size = new System.Drawing.Size(327, 21);
            this.txtReplaceWith.TabIndex = 13;
            // 
            // txtLookFor
            // 
            this.txtLookFor.Location = new System.Drawing.Point(12, 24);
            this.txtLookFor.Name = "txtLookFor";
            this.txtLookFor.Size = new System.Drawing.Size(327, 21);
            this.txtLookFor.TabIndex = 11;
            // 
            // lblReplaceWith
            // 
            this.lblReplaceWith.AutoSize = true;
            this.lblReplaceWith.Location = new System.Drawing.Point(12, 50);
            this.lblReplaceWith.Name = "lblReplaceWith";
            this.lblReplaceWith.Size = new System.Drawing.Size(41, 12);
            this.lblReplaceWith.TabIndex = 12;
            this.lblReplaceWith.Text = "替换为";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "查找：";
            // 
            // checkBox1
            // 
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.Location = new System.Drawing.Point(12, 92);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(104, 24);
            this.checkBox1.TabIndex = 22;
            this.checkBox1.Text = "高亮显示内容";
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox2.Location = new System.Drawing.Point(122, 92);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(104, 22);
            this.checkBox2.TabIndex = 23;
            this.checkBox2.Text = "忽略大小写";
            this.checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox3.Location = new System.Drawing.Point(236, 92);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(104, 22);
            this.checkBox3.TabIndex = 24;
            this.checkBox3.Text = "全字匹配";
            this.checkBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // FormFindAndReplace
            // 
            this.ClientSize = new System.Drawing.Size(352, 153);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnFindPrevious);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.txtReplaceWith);
            this.Controls.Add(this.txtLookFor);
            this.Controls.Add(this.lblReplaceWith);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFindAndReplace";
            this.Text = "查找与替换";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
