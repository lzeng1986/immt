using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.DataGridViews
{
    [ToolboxItem(false)]
    public class DataGridViewNumericUpDownColumn : DataGridViewColumn
    {
        public DataGridViewNumericUpDownColumn()
            : base(new DataGridViewNumericUpDownCell())
        {
            MaxValue = int.MaxValue;
        }

        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(DataGridViewNumericUpDownCell)))
                {
                    throw new InvalidCastException("Must be a NumericUpDownCell");
                }
                base.CellTemplate = value;
            }
        }
        [DefaultValue(0), Category("Data")]
        public int DefaultValue { get; set; }
        [DefaultValue(0), Category("Data")]
        public int MinValue { get; set; }
        [DefaultValue(int.MaxValue), Category("Data")]
        public int MaxValue { get; set; }
        [DefaultValue(""), Category("Data")]
        public string NullValue { get; set; }

        public override object Clone()
        {
            var col = base.Clone() as DataGridViewNumericUpDownColumn;
            col.DefaultValue = DefaultValue;
            col.MinValue = MinValue;
            col.MaxValue = MaxValue;
            col.NullValue = NullValue;
            return col;
        }
    }

    class DataGridViewNumericUpDownCell : DataGridViewTextBoxCell
    {
        public override void InitializeEditingControl(int rowIndex, object
            initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            var column = base.OwningColumn as DataGridViewNumericUpDownColumn;
            if (column == null)
                return;
            var ctrl = DataGridView.EditingControl as DataGridViewNumericUpDownEditingControl;
            ctrl.Minimum = column.MinValue;
            ctrl.Maximum = column.MaxValue;
            ctrl.Value = Convert.ToDecimal(Value);
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
            get { return typeof(DataGridViewNumericUpDownEditingControl); }
        }
        public override Type ValueType
        {
            get { return typeof(decimal); }
        }
        public override object DefaultNewRowValue
        {
            get { return default(decimal); }
        }
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (Convert.ToDecimal(value) == default(decimal))
                return (base.OwningColumn as DataGridViewNumericUpDownColumn).NullValue;
            return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
        }
    }

    class DataGridViewNumericUpDownEditingControl : NumericUpDown, IDataGridViewEditingControl
    {
        public string NullText { get; set; }
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
        }

        public DataGridView EditingControlDataGridView { get; set; }

        public object EditingControlFormattedValue
        {
            get { return Value.ToString(); }
            set
            {
                if (value is string)
                {
                    Value = decimal.Parse((string)value);
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
        }

        public bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }
        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            EditingControlValueChanged = true;
            EditingControlDataGridView.NotifyCurrentCellDirty(true);
        }
    }
}
