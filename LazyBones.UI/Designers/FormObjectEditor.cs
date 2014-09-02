using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Designers
{
    public partial class FormObjectEditor : Form
    {
        public static string[] ObjectTypeNames =
            new[] { "String", "Numeric", "Decimal", "Boolean", "DateTime", "<null>" };
        private ComboBox cmbEditor = new ComboBox();
        private DateTimePicker dtEditor = new DateTimePicker();
        private TextBox txtEditor = new TextBox();
        Control oldControl = null;

        object editValue;
        public object EditValue
        {
            get { return editValue; }
            set
            {
                if (value != null)
                    CheckValueType(value.GetType());
                editValue = value;
                SetEditor();
            }
        }

        Dictionary<string, Control> controlCache = new Dictionary<string, Control>();
        Dictionary<Type, Control> controls = new Dictionary<Type, Control>()
        {
            {typeof(int),new TextBox()},
            {typeof(short),new TextBox()},
            {typeof(long),new TextBox()},
            {typeof(string),new TextBox()},
            {typeof(bool),new ComboBox()},
            {typeof(DateTime),new DateTimePicker()},
            {typeof(int),new TextBox()},
            {typeof(int),new TextBox()},
            {typeof(int),new TextBox()},
        };
        readonly ObjectType objType;
        public FormObjectEditor()
        {
            InitializeComponent();

            this.comboBoxType.Items.AddRange(ObjectTypeNames);

            cmbEditor.Items.AddRange(new[] { "true", "false" });
            cmbEditor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEditor.SelectedIndex = 0;
            dtEditor.Value = DateTime.Now;

            controlCache["String"] = txtEditor;
            controlCache["Numeric"] = txtEditor;
            controlCache["Decimal"] = txtEditor;
            controlCache["Boolean"] = cmbEditor;
            controlCache["DateTime"] = dtEditor;
            controlCache["<null>"] = txtEditor;
            //objType = GetObjectType(obj);
            this.comboBoxType.SelectedIndex = (int)objType;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }
        void CheckValueType(Type type)
        {
            if (type.IsPrimitive)
                return;
            if (type.IsEnum)
                return;
            if (type == typeof(DateTime))
                return;
            throw new ArgumentException("编辑器不得编辑类型:" + type.Name);
        }
        void SetEditor()
        {
            //var valueType = editValue
            //if()
        }
        public static ObjectType GetObjectType(object val)
        {
            if (val == null)
            {
                return ObjectType.Null;
            }
            string str = "";
            try
            {
                str = val.GetType().Name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
            }
            switch (str)
            {
                case "boolean":
                    return ObjectType.Boolean;
                case "datetime":
                    return ObjectType.DateTime;
                case "decimal":
                    return ObjectType.Decimal;
                case "string":
                    return ObjectType.String;
                case "byte":
                case "int16":
                case "int32":
                case "int64":
                case "single":
                case "double":
                    return ObjectType.Numeric;
                default:
                    return ObjectType.String;
            }
        }
        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ctrl = controlCache[this.comboBoxType.Text];
            if (ctrl != oldControl)
            {
                this.Controls.Remove(oldControl);
                this.Controls.Add(ctrl);
                ctrl.Location = new Point(comboBoxType.Location.X, labelValue.Location.Y + (labelValue.Height - ctrl.Height) / 2);
                ctrl.Width = comboBoxType.Width;
                oldControl = ctrl;
            }

            switch ((ObjectType)comboBoxType.SelectedIndex)
            {
                case ObjectType.Decimal:
                case ObjectType.Numeric:
                case ObjectType.String:
                    ctrl.Text = EditValue == null ? null : EditValue.ToString();
                    break;
                case ObjectType.Null:
                    ctrl.Text = "<null>";
                    break;
                case ObjectType.Boolean:
                    (ctrl as ComboBox).SelectedIndex = (EditValue is bool) && (EditValue.Equals(false)) ? 1 : 0;
                    break;
                case ObjectType.DateTime:
                    (ctrl as DateTimePicker).Value = (EditValue is DateTime) ? (DateTime)EditValue : DateTime.Now;
                    break;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                var ctrl = controlCache[this.comboBoxType.Text];
                switch ((ObjectType)comboBoxType.SelectedIndex)
                {
                    case ObjectType.String:
                        EditValue = ctrl.Text;
                        break;
                    case ObjectType.Numeric:
                        EditValue = Convert.ToDouble(ctrl.Text);
                        break;
                    case ObjectType.Decimal:
                        EditValue = Convert.ToDecimal(ctrl.Text);
                        break;
                    case ObjectType.Boolean:
                        EditValue = Convert.ToBoolean(ctrl.Text);
                        break;
                    case ObjectType.DateTime:
                        EditValue = (ctrl as DateTimePicker).Value;
                        break;
                    case ObjectType.Null:
                        EditValue = null;
                        break;
                    default:
                        break;
                }
            }
            catch (FormatException ex)
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    MessageBox.Show(ex.Message);
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }
    }
    public enum ObjectType : int
    {
        String = 0,
        Numeric,
        Decimal,
        Boolean,
        DateTime,
        Null
    }
}
