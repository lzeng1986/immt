using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.DataGridViews
{
    [ToolboxItem(false)]
    public class DataGridViewRichTextBoxColumn : DataGridViewColumn
    {
        public DataGridViewRichTextBoxColumn()
            : base(new DataGridViewRichTextBoxCell())
        {
        }
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewRichTextBoxCell)))
                    throw new InvalidCastException("Must be a DataGridViewRichTextBoxCell");
                base.CellTemplate = value;
            }
        }
    }
    class DataGridViewRichTextBoxCell : DataGridViewTextBoxCell
    {
        public override void InitializeEditingControl(int rowIndex, object
            initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            var ctrl = DataGridView.EditingControl as DataGridViewRichTextBoxEditControl;
            ctrl.Text = (string)this.Value;
        }
        protected override object GetValue(int rowIndex)
        {
            var ctrl = DataGridView.EditingControl as IDataGridViewEditingControl;
            if (ctrl != null)
                return base.GetValue(ctrl.EditingControlRowIndex);
            return base.GetValue(rowIndex);
        }
        public override Type EditType
        {
            get { return typeof(DataGridViewRichTextBoxEditControl); }
        }

        public override Type ValueType
        {
            get { return typeof(string); }
        }

        public override object DefaultNewRowValue
        {
            get { return string.Empty; }
        }
    }
    class DataGridViewRichTextBoxEditControl : DropDownRichTextBox, IDataGridViewEditingControl
    {
        public DataGridViewRichTextBoxEditControl()
        {
        }
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
        }

        public DataGridView EditingControlDataGridView { get; set; }

        public object EditingControlFormattedValue
        {
            get { return Text; }
            set
            {
                if (value is string)
                {
                    Text = (string)value;
                }
            }
        }

        public int EditingControlRowIndex { get; set; }

        public bool EditingControlValueChanged { get; set; }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {

            return !dataGridViewWantsInputKey;
        }

        public Cursor EditingPanelCursor
        {
            get { return base.Cursor; }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
            OpenDropDown();
        }
        protected override void OnDropDownClosed()
        {
            base.OnDropDownClosed();
            EditingControlValueChanged = true;
            EditingControlDataGridView.NotifyCurrentCellDirty(true);
        }

        public bool RepositionEditingControlOnValueChange
        {
            get { return true; }
        }
    }
}
