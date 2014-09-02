using System;
using System.Windows.Forms;

namespace LazyBones.UI.Designers
{
    public partial class FormTreeViewEditor : Form
    {
        public FormTreeViewEditor()
        {
            InitializeComponent();
        }

        public Control EditControl { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (EditControl != null)
            {
                EditControl.Dock = DockStyle.Fill;
                panelControl.Controls.Add(EditControl);
                propertyGridControl.SelectedObject = EditControl;
            }
            base.OnLoad(e);
        }
    }
}
