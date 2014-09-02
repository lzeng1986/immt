using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.DataGridViews
{
    [ToolboxItem(false)]
    public class DataGridViewComboBoxExColumn : DataGridViewColumn
    {
        public DataGridViewComboBoxExColumn()
            : base(new DataGridViewComboBoxExCell())
        {

        }
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewComboBoxExCell)))
                    throw new InvalidCastException("Must be a DataGridViewComboBoxExCell");
                base.CellTemplate = value;
            }
        }

        DataGridViewComboBoxExCell ComboBoxCellTemplate
        {
            get { return (DataGridViewComboBoxExCell)this.CellTemplate; }
        }

        [DefaultValue(""), Category("Data")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [AttributeProvider(typeof(IListSource))]
        public IList DataSource
        {
            get { return ComboBoxCellTemplate.DataSource; }
            set { ComboBoxCellTemplate.DataSource = value; }
        }

        [DefaultValue(""), Category("Data")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get { return ComboBoxCellTemplate.DisplayMember; }
            set { ComboBoxCellTemplate.DisplayMember = value; }
        }

        [DefaultValue(""), Category("Data")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get { return ComboBoxCellTemplate.ValueMember; }
            set { ComboBoxCellTemplate.ValueMember = value; }
        }
        [DefaultValue(""), Category("Data")]
        public string NullText
        {
            get { return ComboBoxCellTemplate.NullText; }
            set { ComboBoxCellTemplate.NullText = value; }
        }

    }

    class DataGridViewComboBoxExCell : DataGridViewTextBoxCell
    {
        public override Type EditType
        {
            get { return typeof(ComboBoxExCellControl); }
        }
        public string NullText { get; set; }
        BindingContext bindingContext = new BindingContext();
        IList dataSource;
        CurrencyManager dataManager;
        public IList DataSource
        {
            get { return dataSource; }
            set
            {
                if (dataSource == value)
                    return;
                dataSource = value;
                if (dataSource != null)
                {
                    dataManager = bindingContext[dataSource] as CurrencyManager;
                }
                else
                {
                    dataManager = null;
                }
            }
        }
        string displayMember;
        PropertyDescriptor displayMemberProperty;
        public string DisplayMember
        {
            get { return displayMember; }
            set
            {
                if (displayMember == value)
                    return;
                displayMember = value;
                if (dataManager != null)
                {
                    var props = dataManager.GetItemProperties();
                    displayMemberProperty = props.Find(displayMember, true);
                }
                else
                {
                    displayMemberProperty = null;
                }
            }
        }
        string valueMember;
        PropertyDescriptor valueMemberProperty;
        public string ValueMember
        {
            get { return valueMember; }
            set
            {
                if (valueMember == value)
                    return;
                valueMember = value;
                if (dataManager != null)
                {
                    var props = dataManager.GetItemProperties();
                    valueMemberProperty = props.Find(valueMember, true);
                }
                else
                {
                    valueMemberProperty = null;
                }
            }
        }
        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            var ctrl = DataGridView.EditingControl as ComboBoxExCellControl;
            ctrl.SelectedIndex = -1;
            ctrl.DataSource = DataSource;
            ctrl.DisplayMember = DisplayMember;
            ctrl.ValueMember = ValueMember;
            ctrl.SelectedIndex = GetValueIndex(Value);
        }
        public override object Clone()
        {
            var cell = base.Clone() as DataGridViewComboBoxExCell;
            cell.DataSource = DataSource;
            cell.DisplayMember = DisplayMember;
            cell.ValueMember = ValueMember;
            cell.NullText = NullText;
            return cell;
        }
        int GetValueIndex(object value)
        {
            if (value == null)
                return -1;
            var list = dataManager.List;
            for (var i = 0; i < list.Count; i++)
            {
                var tmp = valueMemberProperty.GetValue(list[i]);
                if (tmp.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (dataManager == null)
                return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            var descriptor = displayMemberProperty != null ? displayMemberProperty : valueMemberProperty;
            if (descriptor == null)
                return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
            var ind = GetValueIndex(value);
            if (ind == -1)
                return NullText;
            return descriptor.GetValue(dataManager.List[ind]);
        }
        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            return EditControl.SelectedValue;
        }
        ComboBoxExCellControl EditControl { get { return DataGridView.EditingControl as ComboBoxExCellControl; } }
    }

    class ComboBoxExCellControl : DropDownListBox, IDataGridViewEditingControl
    {
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

        public bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }
    }
}
