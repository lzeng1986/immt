using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace LazyBones.UI.Designers
{
    [Designer(typeof(EnumCheckedBoxListDesigner))]
    public class EnumCheckedBoxList : CheckedListBox, ISupportInitialize
    {
        public EnumCheckedBoxList()
        {
            this.CheckOnClick = true;
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Enum EnumValue
        {
            get
            {
                return (Enum)Enum.ToObject(enumType, GetCurrentValue());
            }
            set
            {
                EnumType = value.GetType();
                this.Items.Clear();
                this.Items.AddRange(displaySource);
                var intValue = (int)Convert.ChangeType(value, typeof(int));
                var ind = Array.IndexOf(valueSource, intValue);
                UpdateItems(ind, CheckState.Checked);
            }
        }

        Type enumType = null;
        [
        Category("Data"), Description("指定绑定的枚举类型"),
        Editor(typeof(EnumSelectEditor), typeof(System.Drawing.Design.UITypeEditor)),
        DefaultValue((Type)null),
        TypeConverter(typeof(EnumTypeConverter))
        ]
        public Type EnumType
        {
            get
            {
                return enumType;
            }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("输入类型必须为枚举类型");
                enumType = value;
                if (enumType.IsDefined(typeof(FlagsAttribute), false))
                {
                    updating = UpdateWithFlags;
                }
                else
                {
                    updating = UpdateWithoutFlags;
                }
                displaySource = Enum.GetNames(value);
                valueSource = Enum.GetValues(value).Cast<object>().Select(v => (int)Convert.ChangeType(v, typeof(int))).ToArray();
                this.Items.Clear();
                this.Items.AddRange(displaySource);
                this.Height = ItemHeight * (Items.Count + 2);
            }
        }
        Action<int, CheckState> updating;
        int[] valueSource;
        string[] displaySource;
        bool isUpdating = false;
        protected override void OnItemCheck(ItemCheckEventArgs ice)
        {
            base.OnItemCheck(ice);
            if (isUpdating)//需要用isUpdating控制，否则会产生StackOverFlow错误
                return;
            UpdateItems(ice.Index, ice.NewValue);
        }
        void UpdateWithoutFlags(int index, CheckState state)//不带Flags特性的枚举类型，每次只能选择一项
        {
            for (int i = 0; i < Items.Count; i++)
            {
                SetItemChecked(i, false);
            }
            SetItemChecked(index, state == CheckState.Checked);
        }
        void UpdateWithFlags(int index, CheckState state)//带Flags特性的枚举类型，可以选择多项
        {
            //控制逻辑：
            //1.现把现选择的值或求和，在根据当前选择项的状态，选择是将当前项添加至和或者从和中剔除
            //2.根据求得的和，检查每一项的值是否在当前和中，用与运算检查，如果值为零，则检查和是否为零
            //3.对于当前选择项的值为零的情况，使用不带Flags特性的枚举类型的方式编辑，即清空其它所有选择项，只设置当前项的状态
            var value = valueSource[index];
            if (value == 0)
            {
                UpdateWithoutFlags(index, state);
                return;
            }
            var sum = GetCurrentValue();
            if (state == CheckState.Checked)
            {
                sum |= value;
            }
            else
            {
                sum = sum & (~value);
            }
            for (int i = 0; i < Items.Count; i++)
            {
                if (valueSource[i] == 0)
                    SetItemChecked(i, value == 0);
                else if ((valueSource[i] & sum) == valueSource[i])
                    SetItemChecked(i, true);
                else
                    SetItemChecked(i, false);
            }
        }
        int GetCurrentValue()
        {
            var sum = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                if (GetItemChecked(i))
                    sum |= valueSource[i];
            }
            return sum;
        }
        void UpdateItems(int index, CheckState state)
        {
            isUpdating = true;
            updating(index, state);
            isUpdating = false;
        }

        public void BeginInit()
        {

        }

        public void EndInit()
        {
            this.Items.Clear();
            this.Items.AddRange(displaySource);
        }
    }
}
